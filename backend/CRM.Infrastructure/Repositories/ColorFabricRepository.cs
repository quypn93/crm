using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ColorFabricRepository : Repository<ColorFabric>, IColorFabricRepository
{
    public ColorFabricRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<ColorFabric?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(cf => cf.Name == name);
    }

    public async Task<(IEnumerable<ColorFabric> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(cf =>
                cf.Name.ToLower().Contains(search) ||
                (cf.Description != null && cf.Description.ToLower().Contains(search)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(cf => cf.Name)
                : query.OrderByDescending(cf => cf.Name),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(cf => cf.CreatedAt)
                : query.OrderByDescending(cf => cf.CreatedAt),
            _ => query.OrderBy(cf => cf.Name)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
