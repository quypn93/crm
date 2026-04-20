using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task DeleteItemsByOrderIdAsync(Guid orderId);
    Task AddItemsAsync(IEnumerable<OrderItem> items);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? customerId,
        Guid? dealId,
        Guid? assignedTo,
        Guid? createdBy,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        DateTime? orderDateFrom,
        DateTime? orderDateTo,
        decimal? minAmount,
        decimal? maxAmount,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Order>> GetByDealAsync(Guid dealId);
    Task<IEnumerable<Order>> GetByAssignedUserAsync(Guid userId);
    Task<string> GenerateOrderNumberAsync();
    Task<int> GetOrderCountByStatusAsync(OrderStatus status);
    Task<decimal> GetTotalRevenueAsync(DateTime? from = null, DateTime? to = null);

    // Role-specific dashboard methods
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int limit = 20);
    Task<IEnumerable<Order>> GetOrdersByStatusesAsync(OrderStatus[] statuses, int limit = 20);
    Task<int> GetTotalItemsCountByStatusesAsync(OrderStatus[] statuses);
    Task<decimal> GetTotalAmountByStatusAsync(OrderStatus status);
    Task<int> GetOrdersCompletedTodayByStatusAsync(OrderStatus status);
    Task<int> GetOrdersDeliveredTodayAsync();
}
