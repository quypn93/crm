using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface IShirtComponentRepository : IRepository<ShirtComponent>
{
    Task<IEnumerable<ShirtComponent>> GetByTypeAsync(ComponentType type);
    Task<IEnumerable<ShirtComponent>> GetActiveByTypeAsync(ComponentType type);
    Task<IEnumerable<ShirtComponent>> GetByColorFabricIdAsync(Guid colorFabricId);
    Task<(IEnumerable<ShirtComponent> Items, int TotalCount)> GetPagedAsync(
        string? search,
        ComponentType? type,
        Guid? colorFabricId,
        bool? includeDeleted,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
}
