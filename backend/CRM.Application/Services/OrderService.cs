using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Order;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IQrCodeService _qrCodeService;
    private readonly IOrderProductionService _orderProductionService;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper,
        IQrCodeService qrCodeService, IOrderProductionService orderProductionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _qrCodeService = qrCodeService;
        _orderProductionService = orderProductionService;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<PaginatedResult<OrderDto>> GetPagedAsync(OrderFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
            filter.Search,
            filter.CustomerId,
            filter.DealId,
            filter.AssignedTo,
            filter.CreatedBy,
            filter.Status,
            filter.PaymentStatus,
            filter.OrderDateFrom,
            filter.OrderDateTo,
            filter.MinAmount,
            filter.MaxAmount,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<OrderDto>>(items);
        return PaginatedResult<OrderDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerAsync(Guid customerId)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerAsync(customerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetByDealAsync(Guid dealId)
    {
        var orders = await _unitOfWork.Orders.GetByDealAsync(dealId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetByAssignedUserAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid userId)
    {
        // Verify customer if provided
        Customer? customer = null;
        if (dto.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId.Value);
            if (customer == null)
                throw new KeyNotFoundException("Không tìm thấy khách hàng.");
        }

        // Verify deal exists if provided
        if (dto.DealId.HasValue)
        {
            var deal = await _unitOfWork.Deals.GetByIdAsync(dto.DealId.Value);
            if (deal == null)
                throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        // Production days option → compute CompletionDate and ReturnDate automatically
        ProductionDaysOption? prodOption = null;
        DateTime? completionDate = dto.CompletionDate;
        if (dto.ProductionDaysOptionId.HasValue)
        {
            prodOption = await _unitOfWork.ProductionDaysOptions.GetByIdAsync(dto.ProductionDaysOptionId.Value);
            if (prodOption != null)
                completionDate = DateTime.UtcNow.AddDays(prodOption.Days);
        }
        var returnDate = completionDate?.AddDays(1);

        var order = new Order
        {
            OrderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync(),
            CustomerId = dto.CustomerId,
            CustomerName = customer?.Name ?? dto.CustomerName,
            DealId = dto.DealId,
            Status = OrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            CompletionDate = completionDate,
            ReturnDate = returnDate,
            ProductionDaysOptionId = dto.ProductionDaysOptionId,
            ProductionDays = prodOption?.Days,
            DepositCode = dto.DepositCode,
            ShippingAddress = dto.ShippingAddress ?? customer?.Address,
            ShippingCity = dto.ShippingCity ?? customer?.City,
            ShippingPhone = dto.ShippingPhone ?? customer?.Phone,
            ShippingNotes = dto.ShippingNotes,
            DiscountPercent = dto.DiscountPercent,
            TaxPercent = dto.TaxPercent,
            Notes = dto.Notes,
            InternalNotes = dto.InternalNotes,
            StyleNotes = dto.StyleNotes,
            CreatedByUserId = userId,
            AssignedToUserId = dto.AssignedToUserId ?? userId,
            DesignerUserId = dto.DesignerUserId
        };

        // Add items - resolve names from pool ids
        foreach (var itemDto in dto.Items)
        {
            var item = await BuildOrderItemAsync(itemDto);
            CalculateItemTotal(item);
            order.Items.Add(item);
        }

        CalculateOrderTotals(order);

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Sinh QR ngay khi tạo đơn để có thể in và dán vào ảnh template
        order.QrCodeToken = _qrCodeService.GenerateToken(order.Id);
        order.QrCodeImageBase64 = await _qrCodeService.GenerateQrCodeBase64Async(order.Id, order.OrderNumber);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new InvalidOperationException("Không thể tạo đơn hàng.");
    }

    public async Task<OrderDto> UpdateAsync(UpdateOrderDto dto, Guid userId)
    {
        // Load Order KHÔNG include Items — tránh EF tracking toàn subgraph,
        // items cũ sẽ được xoá bằng ExecuteDelete (bypass change tracker).
        var order = await _unitOfWork.Orders.GetByIdAsync(dto.Id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Chỉ có thể chỉnh sửa đơn hàng ở trạng thái Nháp hoặc Đã xác nhận.");

        // Production days option → recompute dates
        ProductionDaysOption? prodOption = null;
        DateTime? completionDate = dto.CompletionDate;
        if (dto.ProductionDaysOptionId.HasValue)
        {
            prodOption = await _unitOfWork.ProductionDaysOptions.GetByIdAsync(dto.ProductionDaysOptionId.Value);
            if (prodOption != null)
                completionDate = order.OrderDate.AddDays(prodOption.Days);
        }

        // Update basic info — order được tracked, EF tự detect thay đổi field.
        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        order.CompletionDate = completionDate;
        order.ReturnDate = completionDate?.AddDays(1);
        order.ProductionDaysOptionId = dto.ProductionDaysOptionId;
        order.ProductionDays = prodOption?.Days ?? order.ProductionDays;
        order.DepositCode = dto.DepositCode;
        order.ShippingAddress = dto.ShippingAddress;
        order.ShippingCity = dto.ShippingCity;
        order.ShippingPhone = dto.ShippingPhone;
        order.ShippingNotes = dto.ShippingNotes;
        order.DiscountPercent = dto.DiscountPercent;
        order.TaxPercent = dto.TaxPercent;
        order.Notes = dto.Notes;
        order.InternalNotes = dto.InternalNotes;
        order.StyleNotes = dto.StyleNotes;
        order.AssignedToUserId = dto.AssignedToUserId;
        order.DesignerUserId = dto.DesignerUserId;

        // Xoá toàn bộ items cũ bằng ExecuteDelete (raw SQL DELETE WHERE OrderId=X),
        // không đi qua change tracker → tránh lỗi "0 rows affected" của pattern .Clear().
        await _unitOfWork.Orders.DeleteItemsByOrderIdAsync(order.Id);

        // Build items mới rồi add trực tiếp vào DbSet.OrderItems (không đụng order.Items navigation).
        var newItems = new List<OrderItem>();
        foreach (var itemDto in dto.Items)
        {
            var item = await BuildOrderItemAsync(itemDto);
            item.OrderId = order.Id;
            CalculateItemTotal(item);
            newItems.Add(item);
        }
        if (newItems.Count > 0)
            await _unitOfWork.Orders.AddItemsAsync(newItems);

        // Tính tổng dựa trên list items mới (vì order.Items vẫn là collection cũ/rỗng)
        order.SubTotal = newItems.Sum(i => i.LineTotal);
        order.DiscountAmount = order.SubTotal * order.DiscountPercent / 100;
        var afterDiscount = order.SubTotal - order.DiscountAmount;
        order.TaxAmount = afterDiscount * order.TaxPercent / 100;
        order.TotalAmount = afterDiscount + order.TaxAmount;

        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new InvalidOperationException("Không thể cập nhật đơn hàng.");
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        // Validate status transition
        ValidateStatusTransition(order.Status, dto.Status);

        var previousStatus = order.Status;
        order.Status = dto.Status;

        // Khi chuyển sang InProduction: sinh QR và khởi tạo các bước sản xuất
        if (dto.Status == OrderStatus.InProduction && previousStatus != OrderStatus.InProduction)
        {
            if (string.IsNullOrEmpty(order.QrCodeToken))
            {
                order.QrCodeToken = _qrCodeService.GenerateToken(order.Id);
                order.QrCodeImageBase64 = await _qrCodeService.GenerateQrCodeBase64Async(order.Id, order.OrderNumber);
            }
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _orderProductionService.InitializeStepsAsync(order.Id);
        }

        if (dto.Status == OrderStatus.Delivered)
            order.ActualDeliveryDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            order.InternalNotes = $"{order.InternalNotes}\n[{DateTime.UtcNow:dd/MM/yyyy HH:mm}] {dto.Notes}";

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        order.PaymentStatus = dto.PaymentStatus;
        order.PaymentMethod = dto.PaymentMethod;
        order.PaidAmount = dto.PaidAmount;
        order.PaymentDate = dto.PaymentDate ?? DateTime.UtcNow;

        // Auto-update payment status based on paid amount
        if (dto.PaidAmount >= order.TotalAmount)
            order.PaymentStatus = PaymentStatus.Paid;
        else if (dto.PaidAmount > 0)
            order.PaymentStatus = PaymentStatus.PartialPaid;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ có thể xóa đơn hàng ở trạng thái Nháp.");

        _unitOfWork.Orders.Delete(order);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<OrderDto> CreateFromDealAsync(Guid dealId, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId);
        if (deal == null)
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");

        var customer = await _unitOfWork.Customers.GetByIdAsync(deal.CustomerId);
        if (customer == null)
            throw new KeyNotFoundException("Không tìm thấy khách hàng.");

        var createDto = new CreateOrderDto
        {
            CustomerId = deal.CustomerId,
            DealId = dealId,
            ShippingAddress = customer.Address,
            ShippingCity = customer.City,
            ShippingPhone = customer.Phone,
            AssignedToUserId = deal.AssignedToUserId ?? userId,
            Notes = $"Tạo từ giao dịch: {deal.Title}",
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    Quantity = 1,
                    UnitPrice = deal.Value,
                    Notes = deal.Notes
                }
            }
        };

        return await CreateAsync(createDto, userId);
    }

    public async Task<OrderSummaryDto> GetSummaryAsync(Guid? userId = null)
    {
        var totalOrders = await _unitOfWork.Orders.CountAsync(o => true);
        var draftOrders = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.Draft);
        var inProgressCount = await _unitOfWork.Orders.CountAsync(o =>
            o.Status == OrderStatus.Confirmed ||
            o.Status == OrderStatus.InProduction ||
            o.Status == OrderStatus.QualityCheck ||
            o.Status == OrderStatus.ReadyToShip ||
            o.Status == OrderStatus.Shipping);
        var completedOrders = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.Completed);
        var totalRevenue = await _unitOfWork.Orders.GetTotalRevenueAsync();
        var pendingPayment = await _unitOfWork.Orders.SumAsync(o =>
            o.PaymentStatus != PaymentStatus.Paid && o.Status != OrderStatus.Cancelled,
            o => o.TotalAmount - o.PaidAmount);

        return new OrderSummaryDto
        {
            TotalOrders = totalOrders,
            DraftOrders = draftOrders,
            InProgressOrders = inProgressCount,
            CompletedOrders = completedOrders,
            TotalRevenue = totalRevenue,
            PendingPayment = pendingPayment
        };
    }

    private async Task<OrderItem> BuildOrderItemAsync(CreateOrderItemDto itemDto)
    {
        var item = new OrderItem
        {
            CollectionId = itemDto.CollectionId,
            ProductCode = itemDto.ProductCode,
            Description = itemDto.Description,
            Size = itemDto.Size,
            MainColorId = itemDto.MainColorId,
            AccentColorId = itemDto.AccentColorId,
            MaterialId = itemDto.MaterialId,
            FormId = itemDto.FormId,
            SpecificationId = itemDto.SpecificationId,
            Quantity = itemDto.Quantity,
            Unit = itemDto.Unit,
            UnitPrice = itemDto.UnitPrice,
            DiscountPercent = itemDto.DiscountPercent,
            Notes = itemDto.Notes
        };

        if (itemDto.CollectionId.HasValue)
        {
            var col = await _unitOfWork.Collections.GetByIdAsync(itemDto.CollectionId.Value);
            item.CollectionName = col?.Name;
        }
        if (itemDto.MainColorId.HasValue)
            item.MainColorName = (await _unitOfWork.ColorFabrics.GetByIdAsync(itemDto.MainColorId.Value))?.Name;
        if (itemDto.AccentColorId.HasValue)
            item.AccentColorName = (await _unitOfWork.ColorFabrics.GetByIdAsync(itemDto.AccentColorId.Value))?.Name;
        if (itemDto.MaterialId.HasValue)
            item.MaterialName = (await _unitOfWork.Materials.GetByIdAsync(itemDto.MaterialId.Value))?.Name;
        if (itemDto.FormId.HasValue)
            item.FormName = (await _unitOfWork.ProductForms.GetByIdAsync(itemDto.FormId.Value))?.Name;
        if (itemDto.SpecificationId.HasValue)
            item.SpecificationName = (await _unitOfWork.ProductSpecifications.GetByIdAsync(itemDto.SpecificationId.Value))?.Name;
        return item;
    }

    private static void CalculateItemTotal(OrderItem item)
    {
        var subtotal = item.Quantity * item.UnitPrice;
        item.DiscountAmount = subtotal * item.DiscountPercent / 100;
        item.LineTotal = subtotal - item.DiscountAmount;
    }

    private static void CalculateOrderTotals(Order order)
    {
        order.SubTotal = order.Items.Sum(i => i.LineTotal);
        order.DiscountAmount = order.SubTotal * order.DiscountPercent / 100;
        var afterDiscount = order.SubTotal - order.DiscountAmount;
        order.TaxAmount = afterDiscount * order.TaxPercent / 100;
        order.TotalAmount = afterDiscount + order.TaxAmount;
    }

    private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            { OrderStatus.Draft, new[] { OrderStatus.Confirmed, OrderStatus.Cancelled } },
            { OrderStatus.Confirmed, new[] { OrderStatus.InProduction, OrderStatus.Cancelled } },
            { OrderStatus.InProduction, new[] { OrderStatus.QualityCheck, OrderStatus.Cancelled } },
            { OrderStatus.QualityCheck, new[] { OrderStatus.ReadyToShip, OrderStatus.InProduction } },
            { OrderStatus.ReadyToShip, new[] { OrderStatus.Shipping } },
            { OrderStatus.Shipping, new[] { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new[] { OrderStatus.Completed } },
            { OrderStatus.Completed, Array.Empty<OrderStatus>() },
            { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
        };

        if (!validTransitions.ContainsKey(current) || !validTransitions[current].Contains(next))
        {
            throw new InvalidOperationException($"Không thể chuyển từ trạng thái '{current}' sang '{next}'.");
        }
    }

    public async Task<OrderDto> SetDesignImageAsync(Guid id, string imageUrl)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        order.DesignImageUrl = imageUrl;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<OrderDto?> GetByQrTokenAsync(string token)
    {
        var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.QrCodeToken == token);
        if (order == null) return null;
        return await GetByIdAsync(order.Id);
    }

    /// <summary>Sinh lại QR cho đơn hàng (dùng cho các đơn cũ chưa có QR)</summary>
    public async Task<OrderDto> GenerateQrAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        order.QrCodeToken = _qrCodeService.GenerateToken(order.Id);
        order.QrCodeImageBase64 = await _qrCodeService.GenerateQrCodeBase64Async(order.Id, order.OrderNumber);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Lỗi khi tải đơn hàng.");
    }
}
