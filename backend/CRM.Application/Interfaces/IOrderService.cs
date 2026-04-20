using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Order;
using CRM.Core.Enums;

namespace CRM.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
    Task<PaginatedResult<OrderDto>> GetPagedAsync(OrderFilterDto filter);
    Task<IEnumerable<OrderDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<OrderDto>> GetByDealAsync(Guid dealId);
    Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid userId);
    Task<OrderDto> UpdateAsync(UpdateOrderDto dto, Guid userId);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, Guid userId);
    Task<OrderDto> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<OrderDto> CreateFromDealAsync(Guid dealId, Guid userId);
    Task<OrderSummaryDto> GetSummaryAsync(Guid? userId = null);
    Task<OrderDto> GenerateQrAsync(Guid id);
    Task<OrderDto> SetDesignImageAsync(Guid id, string imageUrl);
    Task<OrderDto?> GetByQrTokenAsync(string token);
}
