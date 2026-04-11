using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IDesignRepository : IRepository<Design>
{
    Task<Design?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Design>> GetByOrderAsync(Guid orderId);
    Task<IEnumerable<Design>> GetByUserAsync(Guid userId);
    Task<(IEnumerable<Design> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? orderId,
        Guid? colorFabricId,
        Guid? createdByUserId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
}
