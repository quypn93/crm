using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ShirtComponentRepository : Repository<ShirtComponent>, IShirtComponentRepository
{
    public ShirtComponentRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShirtComponent>> GetByTypeAsync(ComponentType type)
    {
        return await _dbSet
            .Include(sc => sc.ColorFabric)
            .Where(sc => sc.Type == type)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShirtComponent>> GetActiveByTypeAsync(ComponentType type)
    {
        return await _dbSet
            .Include(sc => sc.ColorFabric)
            .Where(sc => sc.Type == type && !sc.IsDeleted)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShirtComponent>> GetByColorFabricIdAsync(Guid colorFabricId)
    {
        return await _dbSet
            .Include(sc => sc.ColorFabric)
            .Where(sc => sc.ColorFabricId == colorFabricId && !sc.IsDeleted)
            .OrderBy(sc => sc.Type)
            .ThenBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ShirtComponent> Items, int TotalCount)> GetPagedAsync(
        string? search,
        ComponentType? type,
        Guid? colorFabricId,
        bool? includeDeleted,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet
            .Include(sc => sc.ColorFabric)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(sc => sc.Name.ToLower().Contains(search));
        }

        if (type.HasValue)
            query = query.Where(sc => sc.Type == type.Value);

        if (colorFabricId.HasValue)
            query = query.Where(sc => sc.ColorFabricId == colorFabricId.Value);

        if (includeDeleted != true)
            query = query.Where(sc => !sc.IsDeleted);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(sc => sc.Name)
                : query.OrderByDescending(sc => sc.Name),
            "type" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(sc => sc.Type)
                : query.OrderByDescending(sc => sc.Type),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(sc => sc.CreatedAt)
                : query.OrderByDescending(sc => sc.CreatedAt),
            _ => query.OrderBy(sc => sc.Type).ThenBy(sc => sc.Name)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
