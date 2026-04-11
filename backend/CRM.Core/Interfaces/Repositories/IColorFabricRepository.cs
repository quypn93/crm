using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IColorFabricRepository : IRepository<ColorFabric>
{
    Task<ColorFabric?> GetByNameAsync(string name);
    Task<(IEnumerable<ColorFabric> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
}
