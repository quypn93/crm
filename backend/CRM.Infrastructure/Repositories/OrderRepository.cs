using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Deal)
            .Include(o => o.CreatedByUser)
            .Include(o => o.AssignedToUser)
            .Include(o => o.DesignerUser)
            .Include(o => o.ShipperUser)
            .Include(o => o.ProductionDaysOption)
            .Include(o => o.OrderType)
            .Include(o => o.Design)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    // Xoá toàn bộ OrderItem của một Order bằng ExecuteDelete (bỏ qua EF change tracker)
    // Dùng khi cần "replace all items" trong UpdateAsync — tránh pattern .Clear() + .Add()
    // gây lỗi tracking với cascade required FK.
    public async Task DeleteItemsByOrderIdAsync(Guid orderId)
    {
        await _context.OrderItems
            .Where(i => i.OrderId == orderId)
            .ExecuteDeleteAsync();
    }

    // Thêm items qua DbSet trực tiếp — EF sẽ mark Added và INSERT khi SaveChanges.
    public async Task AddItemsAsync(IEnumerable<OrderItem> items)
    {
        await _context.OrderItems.AddRangeAsync(items);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? customerId,
        Guid? dealId,
        Guid? assignedTo,
        Guid? createdBy,
        Guid? designerUserId,
        Guid? shipperUserId,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        DateTime? orderDateFrom,
        DateTime? orderDateTo,
        decimal? minAmount,
        decimal? maxAmount,
        string? customerName,
        int? minQuantity,
        int? maxQuantity,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Deal)
            .Include(o => o.AssignedToUser)
            .Include(o => o.ShipperUser)
            .Include(o => o.Items)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(search) ||
                o.Customer.Name.ToLower().Contains(search) ||
                (o.Customer.CompanyName != null && o.Customer.CompanyName.ToLower().Contains(search)));
        }

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (dealId.HasValue)
            query = query.Where(o => o.DealId == dealId.Value);

        if (assignedTo.HasValue)
            query = query.Where(o => o.AssignedToUserId == assignedTo.Value);

        if (createdBy.HasValue)
            query = query.Where(o => o.CreatedByUserId == createdBy.Value);

        if (designerUserId.HasValue)
            query = query.Where(o => o.DesignerUserId == designerUserId.Value);

        if (shipperUserId.HasValue)
            query = query.Where(o => o.ShipperUserId == shipperUserId.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (paymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == paymentStatus.Value);

        // Lọc theo ngày tạo đơn (CreatedAt) — khớp cột "Ngày tạo" trên danh sách.
        // Frontend gửi mốc UTC đã quy đổi từ ngày địa phương.
        if (orderDateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= orderDateFrom.Value);

        if (orderDateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= orderDateTo.Value);

        if (minAmount.HasValue)
            query = query.Where(o => o.TotalAmount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(o => o.TotalAmount <= maxAmount.Value);

        // Lọc theo tên khách hàng — khớp cả KH trong danh bạ lẫn tên nhập tay trên đơn
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var name = customerName.Trim().ToLower();
            query = query.Where(o =>
                (o.Customer != null && o.Customer.Name.ToLower().Contains(name)) ||
                (o.CustomerName != null && o.CustomerName.ToLower().Contains(name)));
        }

        // Lọc theo tổng số lượng sản phẩm trong đơn (sum quantity các items)
        if (minQuantity.HasValue)
            query = query.Where(o => o.Items.Sum(i => i.Quantity) >= minQuantity.Value);

        if (maxQuantity.HasValue)
            query = query.Where(o => o.Items.Sum(i => i.Quantity) <= maxQuantity.Value);

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "ordernumber" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(o => o.OrderNumber)
                : query.OrderByDescending(o => o.OrderNumber),
            "customername" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(o => o.Customer.Name)
                : query.OrderByDescending(o => o.Customer.Name),
            "totalamount" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(o => o.TotalAmount)
                : query.OrderByDescending(o => o.TotalAmount),
            "status" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(o => o.Status)
                : query.OrderByDescending(o => o.Status),
            "orderdate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(o => o.OrderDate)
                : query.OrderByDescending(o => o.OrderDate),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByDealAsync(Guid dealId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.DealId == dealId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByAssignedUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.AssignedToUserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    // Đơn thuộc kho mà user là Quản lý kho phụ trách (đã gắn kho ở khâu Vận đơn).
    public async Task<IEnumerable<Order>> GetByWarehouseManagerAsync(Guid userId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Include(o => o.SenderAddress)
            .Where(o => o.SenderAddress != null && o.SenderAddress.AssignedUserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        // Format mới: XA-###### (6 chữ số, tăng dần toàn cục, bắt đầu 000001).
        // Chỉ xét các mã đúng format mới — mã cũ XA-yyMMddNNN (9 chữ số) được bỏ qua.
        const string prefix = "XA-";

        var candidates = await _dbSet
            .Where(o => o.OrderNumber.StartsWith(prefix) && o.OrderNumber.Length == prefix.Length + 6)
            .Select(o => o.OrderNumber)
            .ToListAsync();

        var max = 0;
        foreach (var number in candidates)
        {
            if (int.TryParse(number.Substring(prefix.Length), out var value) && value > max)
                max = value;
        }

        return $"{prefix}{(max + 1):D6}";
    }

    public async Task<int> GetOrderCountByStatusAsync(OrderStatus status)
    {
        return await _dbSet.CountAsync(o => o.Status == status);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet.Where(o => o.Status == OrderStatus.Completed);

        if (from.HasValue)
            query = query.Where(o => o.OrderDate >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.OrderDate <= to.Value);

        return await query.SumAsync(o => o.TotalAmount);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int limit = 20)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusesAsync(OrderStatus[] statuses, int limit = 20)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => statuses.Contains(o.Status))
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetTotalItemsCountByStatusesAsync(OrderStatus[] statuses)
    {
        return await _dbSet
            .Where(o => statuses.Contains(o.Status))
            .SelectMany(o => o.Items)
            .SumAsync(i => i.Quantity);
    }

    public async Task<decimal> GetTotalAmountByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<int> GetOrdersCompletedTodayByStatusAsync(OrderStatus status)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(o => o.Status == status && o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == today)
            .CountAsync();
    }

    public async Task<int> GetOrdersDeliveredTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(o => o.ActualDeliveryDate.HasValue && o.ActualDeliveryDate.Value.Date == today)
            .CountAsync();
    }
}
