using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Order;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IQrCodeService _qrCodeService;
    private readonly IOrderProductionService _orderProductionService;
    private readonly IGhtkShipmentService _ghtkService;
    private readonly IViettelPostShipmentService _viettelPostService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper,
        IQrCodeService qrCodeService, IOrderProductionService orderProductionService,
        IGhtkShipmentService ghtkService, IViettelPostShipmentService viettelPostService,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _qrCodeService = qrCodeService;
        _orderProductionService = orderProductionService;
        _ghtkService = ghtkService;
        _viettelPostService = viettelPostService;
        _logger = logger;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null) return null;

        var dto = _mapper.Map<OrderDto>(order);
        dto.CreatedByManagerName = await ResolveManagerNameAsync(order.CreatedByUserId);
        return dto;
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
        if (order == null) return null;

        var dto = _mapper.Map<OrderDto>(order);
        dto.CreatedByManagerName = await ResolveManagerNameAsync(order.CreatedByUserId);
        return dto;
    }

    public async Task<PaginatedResult<OrderDto>> GetPagedAsync(OrderFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
            filter.Search,
            filter.CustomerId,
            filter.DealId,
            filter.AssignedTo,
            filter.CreatedBy,
            filter.DesignerUserId,
            filter.ShipperUserId,
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
        await EnrichManagerNamesAsync(dtos);
        return PaginatedResult<OrderDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerAsync(Guid customerId)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerAsync(customerId);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        await EnrichManagerNamesAsync(dtos);
        return dtos;
    }

    public async Task<IEnumerable<OrderDto>> GetByDealAsync(Guid dealId)
    {
        var orders = await _unitOfWork.Orders.GetByDealAsync(dealId);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        await EnrichManagerNamesAsync(dtos);
        return dtos;
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetByAssignedUserAsync(userId);
        var dtos = _mapper.Map<List<OrderDto>>(orders);
        await EnrichManagerNamesAsync(dtos);
        return dtos;
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

        // Verify design and derive designer if DesignId provided.
        Guid? resolvedDesignerUserId = dto.DesignerUserId;
        if (dto.DesignId.HasValue)
        {
            var design = await _unitOfWork.Designs.GetByIdAsync(dto.DesignId.Value);
            if (design == null)
                throw new KeyNotFoundException("Không tìm thấy thiết kế.");
            // Designer gắn vào đơn = người đã làm ra design đó.
            resolvedDesignerUserId = design.AssignedToUserId ?? dto.DesignerUserId;
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
        OrderType? orderType = null;
        if (dto.OrderTypeId.HasValue)
        {
            orderType = await _unitOfWork.OrderTypes.FirstOrDefaultAsync(x => x.Id == dto.OrderTypeId.Value);
            if (orderType == null)
                throw new KeyNotFoundException("Không tìm thấy dạng đơn.");
        }

        var order = new Order
        {
            OrderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync(),
            OrderTypeId = dto.OrderTypeId,
            SenderAddressId = dto.SenderAddressId,
            OrderTypeName = orderType?.Name,
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
            DeliveryMethod = dto.DeliveryMethod,
            ShippingContactName = dto.ShippingContactName ?? customer?.Name,
            ShippingPhone = dto.ShippingPhone ?? customer?.Phone,
            ShippingAddress = dto.ShippingAddress ?? customer?.Address,
            ShippingProvinceCode = dto.ShippingProvinceCode,
            ShippingProvinceName = dto.ShippingProvinceName,
            ShippingWardCode = dto.ShippingWardCode,
            ShippingWardName = dto.ShippingWardName,
            ShippingCity = dto.ShippingCity ?? customer?.City,
            ShippingNotes = dto.ShippingNotes,
            ReceiverProvinceId = dto.ReceiverProvinceId,
            ReceiverDistrictId = dto.ReceiverDistrictId,
            ReceiverWardId = dto.ReceiverWardId,
            DiscountPercent = dto.DiscountPercent,
            TaxPercent = dto.TaxPercent,
            Notes = dto.Notes,
            InternalNotes = dto.InternalNotes,
            CustomerNotes = dto.CustomerNotes,
            StyleNotes = dto.StyleNotes,
            CreatedByUserId = userId,
            AssignedToUserId = dto.AssignedToUserId ?? userId,
            DesignerUserId = resolvedDesignerUserId,
            // Shipper chỉ giữ nếu hình thức giao là InHouse — các method khác giao bên ngoài (Vehicle/GHTK) không cần.
            ShipperUserId = dto.DeliveryMethod == DeliveryMethod.InHouse ? dto.ShipperUserId : null,
            DesignId = dto.DesignId
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

        // Khớp mã cọc tiền sale gõ → cộng vào PaidAmount, link DepositTransaction.
        await ApplyDepositToOrderAsync(order);

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
        OrderType? orderType = null;
        if (dto.OrderTypeId.HasValue)
        {
            orderType = await _unitOfWork.OrderTypes.FirstOrDefaultAsync(x => x.Id == dto.OrderTypeId.Value);
            if (orderType == null)
                throw new KeyNotFoundException("Không tìm thấy dạng đơn.");
        }
        order.OrderTypeId = dto.OrderTypeId;
        order.SenderAddressId = dto.SenderAddressId;
        order.OrderTypeName = orderType?.Name;
        order.DeliveryMethod = dto.DeliveryMethod;
        order.ShippingContactName = dto.ShippingContactName;
        order.ShippingPhone = dto.ShippingPhone;
        order.ShippingAddress = dto.ShippingAddress;
        order.ShippingProvinceCode = dto.ShippingProvinceCode;
        order.ShippingProvinceName = dto.ShippingProvinceName;
        order.ShippingWardCode = dto.ShippingWardCode;
        order.ShippingWardName = dto.ShippingWardName;
        order.ShippingCity = dto.ShippingCity;
        order.ShippingNotes = dto.ShippingNotes;
        order.ReceiverProvinceId = dto.ReceiverProvinceId;
        order.ReceiverDistrictId = dto.ReceiverDistrictId;
        order.ReceiverWardId = dto.ReceiverWardId;
        order.DiscountPercent = dto.DiscountPercent;
        order.TaxPercent = dto.TaxPercent;
        order.Notes = dto.Notes;
        order.InternalNotes = dto.InternalNotes;
        order.CustomerNotes = dto.CustomerNotes;
        order.StyleNotes = dto.StyleNotes;
        order.AssignedToUserId = dto.AssignedToUserId;
        // Đổi sang Vehicle/GHTK → clear shipper (kế hoạch giao đã thay đổi).
        order.ShipperUserId = dto.DeliveryMethod == DeliveryMethod.InHouse ? dto.ShipperUserId : null;

        // Nếu chọn design có sẵn → gán designer = người làm ra design.
        if (dto.DesignId.HasValue)
        {
            var design = await _unitOfWork.Designs.GetByIdAsync(dto.DesignId.Value);
            if (design == null)
                throw new KeyNotFoundException("Không tìm thấy thiết kế.");
            order.DesignId = dto.DesignId;
            order.DesignerUserId = design.AssignedToUserId ?? dto.DesignerUserId;
        }
        else
        {
            order.DesignId = null;
            order.DesignerUserId = dto.DesignerUserId;
        }

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

        // Khớp lại mã cọc tiền (nếu sale đổi mã hoặc xoá mã ở edit mode).
        await ApplyDepositToOrderAsync(order);
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

        // Chưa có thiết kế (ảnh hoặc file) thì không thể chuyển sang sản xuất
        if (dto.Status == OrderStatus.InProduction
            && string.IsNullOrWhiteSpace(order.DesignImageUrl)
            && string.IsNullOrWhiteSpace(order.DesignFileUrl))
        {
            throw new InvalidOperationException("Đơn hàng chưa có thiết kế. Cần upload ảnh hoặc file thiết kế trước khi chuyển sang sản xuất.");
        }

        var previousStatus = order.Status;
        order.Status = dto.Status;

        // Hoàn thành đơn = đã thanh toán đủ: tự cộng nốt phần còn nợ và set PaymentStatus = Paid.
        // Ngày thanh toán dời sang thời điểm chốt nếu chưa được ghi.
        if (dto.Status == OrderStatus.Completed)
        {
            if (order.PaidAmount < order.TotalAmount)
                order.PaidAmount = order.TotalAmount;
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentDate ??= DateTime.UtcNow;
        }

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

        // Auto-create GHTK shipment khi chuyển sang ReadyToShip và delivery method = GHTK.
        // Không block status update nếu GHTK lỗi — log và giữ nguyên luồng.
        if (dto.Status == OrderStatus.ReadyToShip
            && order.DeliveryMethod == DeliveryMethod.GHTK
            && string.IsNullOrWhiteSpace(order.GhtkLabel)
            && _ghtkService.IsConfigured)
        {
            try
            {
                await _ghtkService.CreateShipmentAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-create GHTK shipment failed for order {OrderId}", order.Id);
            }
        }

        // Auto-create Viettel Post shipment khi chuyển sang ReadyToShip và delivery method = ViettelPost.
        if (dto.Status == OrderStatus.ReadyToShip
            && order.DeliveryMethod == DeliveryMethod.ViettelPost
            && string.IsNullOrWhiteSpace(order.ViettelPostLabel)
            && _viettelPostService.IsConfigured)
        {
            try
            {
                await _viettelPostService.CreateShipmentAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-create Viettel Post shipment failed for order {OrderId}", order.Id);
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            order.InternalNotes = $"{order.InternalNotes}\n[{DateTime.UtcNow:dd/MM/yyyy HH:mm}] {dto.Notes}";

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> UpdateDeliveryMethodAsync(Guid id, UpdateDeliveryMethodDto dto, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        // Chỉ cho đổi hình thức vận chuyển khi đơn chưa vào khâu vận chuyển
        if (order.Status is OrderStatus.Shipping or OrderStatus.Delivered or OrderStatus.Completed)
            throw new InvalidOperationException("Đơn hàng đã vào khâu vận chuyển, không thể đổi hình thức vận chuyển.");
        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Đơn hàng đã hủy, không thể đổi hình thức vận chuyển.");

        var isChanged = order.DeliveryMethod != dto.DeliveryMethod;

        // GHTK đã ngừng sử dụng — không nhận làm lựa chọn mới (đơn cũ giữ nguyên để truy vết)
        if (isChanged && dto.DeliveryMethod == DeliveryMethod.GHTK)
            throw new InvalidOperationException("Hình thức GHTK đã ngừng sử dụng.");

        // Đã tạo vận đơn thì phải hủy vận đơn trước khi đổi hình thức
        if (isChanged && !string.IsNullOrWhiteSpace(order.GhtkLabel))
            throw new InvalidOperationException("Đơn đã có vận đơn GHTK. Hủy vận đơn GHTK trước khi đổi hình thức vận chuyển.");
        if (isChanged && !string.IsNullOrWhiteSpace(order.ViettelPostLabel))
            throw new InvalidOperationException("Đơn đã có vận đơn Viettel Post. Hủy vận đơn trước khi đổi hình thức vận chuyển.");

        order.DeliveryMethod = dto.DeliveryMethod;
        // NV giao hàng chỉ áp dụng cho hình thức Nhà giao
        order.ShipperUserId = dto.DeliveryMethod == DeliveryMethod.InHouse ? dto.ShipperUserId : null;

        if (isChanged)
        {
            var methodName = dto.DeliveryMethod switch
            {
                DeliveryMethod.InHouse => "Nhà giao",
                DeliveryMethod.Vehicle => "Giao xe",
                DeliveryMethod.GHTK => "Giao Hàng Tiết Kiệm",
                DeliveryMethod.ViettelPost => "Viettel Post",
                _ => dto.DeliveryMethod.ToString()
            };
            order.InternalNotes = $"{order.InternalNotes}\n[{DateTime.UtcNow:dd/MM/yyyy HH:mm}] Đổi hình thức vận chuyển sang: {methodName}".Trim();
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Không thể cập nhật hình thức vận chuyển.");
    }

    public async Task<OrderDto> UpdateDepositCodeAsync(Guid id, UpdateDepositCodeDto dto, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        // Mã cọc tiền sửa được ở mọi trạng thái — ApplyDeposit tự cộng/trả tiền khi đổi/xoá mã.
        order.DepositCode = string.IsNullOrWhiteSpace(dto.DepositCode) ? null : dto.DepositCode.Trim();

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await ApplyDepositToOrderAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Không thể cập nhật mã cọc tiền.");
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

    /// <summary>
    /// Khi sale điền "Mã cọc tiền" vào đơn: tra DepositTransaction theo Code (case-insensitive),
    /// cộng số tiền cọc vào order.PaidAmount, đồng thời link MatchedOrderId hai chiều.
    /// Nếu xoá mã / đổi sang mã khác → trả lại deposit cũ và clear PaidAmount tương ứng.
    /// Bỏ qua deposit đã match cho order khác (tránh sale dùng trùng mã).
    /// </summary>
    private async Task ApplyDepositToOrderAsync(Order order)
    {
        // Trả deposit cũ (nếu có) khi mã hiện tại không còn trỏ về deposit đó nữa.
        var previous = await _unitOfWork.DepositTransactions.FirstOrDefaultAsync(d => d.MatchedOrderId == order.Id);
        if (previous != null)
        {
            var stillCurrent = !string.IsNullOrWhiteSpace(order.DepositCode)
                && string.Equals(previous.Code.Trim(), order.DepositCode!.Trim(), StringComparison.OrdinalIgnoreCase);
            if (!stillCurrent)
            {
                previous.MatchedOrderId = null;
                _unitOfWork.DepositTransactions.Update(previous);
                order.PaidAmount = Math.Max(0, order.PaidAmount - previous.Amount);
            }
        }

        if (string.IsNullOrWhiteSpace(order.DepositCode))
        {
            RecomputePaymentStatus(order);
            return;
        }

        var code = order.DepositCode.Trim();
        var deposit = await _unitOfWork.DepositTransactions.FirstOrDefaultAsync(
            d => d.Code.ToLower() == code.ToLower());

        // Mã không tồn tại hoặc đã match cho đơn khác → để nguyên, không cộng tiền.
        if (deposit == null || (deposit.MatchedOrderId.HasValue && deposit.MatchedOrderId != order.Id))
        {
            RecomputePaymentStatus(order);
            return;
        }

        // Mới khớp lần đầu (chưa link) → cộng tiền + link.
        if (deposit.MatchedOrderId != order.Id)
        {
            deposit.MatchedOrderId = order.Id;
            _unitOfWork.DepositTransactions.Update(deposit);
            order.PaidAmount += deposit.Amount;
        }

        RecomputePaymentStatus(order);
    }

    private static void RecomputePaymentStatus(Order order)
    {
        if (order.PaidAmount >= order.TotalAmount && order.TotalAmount > 0)
            order.PaymentStatus = PaymentStatus.Paid;
        else if (order.PaidAmount > 0)
            order.PaymentStatus = PaymentStatus.PartialPaid;
        else
            order.PaymentStatus = PaymentStatus.Pending;
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
            { OrderStatus.ReadyToShip, new[] { OrderStatus.Shipping, OrderStatus.Completed } },
            { OrderStatus.Shipping, new[] { OrderStatus.Delivered, OrderStatus.Completed } },
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

    public async Task<OrderDto> SetDesignFileAsync(Guid id, string fileUrl, string fileName)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("KhÃ´ng tÃ¬m tháº¥y Ä‘Æ¡n hÃ ng.");
        order.DesignFileUrl = fileUrl;
        order.DesignFileName = fileName;
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
    private async Task EnrichManagerNamesAsync(IEnumerable<OrderDto> orders)
    {
        var cache = new Dictionary<Guid, string?>();
        foreach (var order in orders)
        {
            if (!cache.TryGetValue(order.CreatedByUserId, out var managerName))
            {
                managerName = await ResolveManagerNameAsync(order.CreatedByUserId);
                cache[order.CreatedByUserId] = managerName;
            }

            order.CreatedByManagerName = managerName;
        }
    }

    private async Task<string?> ResolveManagerNameAsync(Guid createdByUserId)
    {
        var creator = await _unitOfWork.Users.GetByIdWithRolesAsync(createdByUserId);
        if (creator == null) return null;

        var creatorRoles = creator.UserRoles
            .Select(ur => ur.Role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var managerRole = ResolveManagerRole(creatorRoles);
        if (string.IsNullOrWhiteSpace(managerRole)) return null;

        if (creatorRoles.Contains(managerRole))
            return creator.FullName;

        var users = await _unitOfWork.Users.GetAllWithRolesAsync();
        var manager = users
            .Where(u => u.IsActive)
            .FirstOrDefault(u => u.UserRoles.Any(ur =>
                ur.Role.Name.Equals(managerRole, StringComparison.OrdinalIgnoreCase)));

        return manager?.FullName;
    }

    private static string? ResolveManagerRole(IReadOnlySet<string> roles)
    {
        if (roles.Contains(RoleNames.SalesManager) || roles.Contains(RoleNames.SalesRep))
            return RoleNames.SalesManager;
        if (roles.Contains(RoleNames.DesignManager) || roles.Contains(RoleNames.Designer))
            return RoleNames.DesignManager;
        if (roles.Contains(RoleNames.ProductionManager)
            || roles.Contains(RoleNames.ProductionStaff)
            || roles.Overlaps(RoleNames.ProductionStageRoles))
            return RoleNames.ProductionManager;
        if (roles.Contains(RoleNames.QualityManager) || roles.Contains(RoleNames.QualityControl))
            return RoleNames.QualityManager;
        if (roles.Contains(RoleNames.DeliveryManager) || roles.Contains(RoleNames.DeliveryStaff))
            return RoleNames.DeliveryManager;
        if (roles.Contains(RoleNames.MarketingManager)
            || roles.Contains(RoleNames.ContentManager)
            || roles.Contains(RoleNames.ContentStaff)
            || roles.Contains(RoleNames.MediaMarketing)
            || roles.Contains(RoleNames.DigitalAds)
            || roles.Contains(RoleNames.Media))
            return RoleNames.MarketingManager;
        if (roles.Contains(RoleNames.Admin))
            return RoleNames.Admin;

        return null;
    }
}
