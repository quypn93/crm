using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdWithDetailsAsync(Guid id);
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? assignedTo,
        bool? isActive,
        string? industry,
        string? city,
        DateTime? createdFrom,
        DateTime? createdTo,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
    Task<IEnumerable<Customer>> GetByAssignedUserAsync(Guid userId);
    Task<int> GetCustomerCountByIndustryAsync(string industry);
    Task<IEnumerable<Customer>> GetAllWithDealsAsync();
}
