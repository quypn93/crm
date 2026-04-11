using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Customer;

namespace CRM.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, Guid userId);
    Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<CustomerDto>> GetByAssignedUserAsync(Guid userId);
}
