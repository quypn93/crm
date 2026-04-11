using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class DesignRepository : Repository<Design>, IDesignRepository
{
    public DesignRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Design?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.ColorFabric)
            .Include(d => d.Order)
                .ThenInclude(o => o!.Customer)
            .Include(d => d.CreatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Design>> GetByOrderAsync(Guid orderId)
    {
        return await _dbSet
            .Include(d => d.ColorFabric)
            .Include(d => d.CreatedByUser)
            .Where(d => d.OrderId == orderId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Design>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(d => d.ColorFabric)
            .Include(d => d.Order)
            .Where(d => d.CreatedByUserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Design> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? orderId,
        Guid? colorFabricId,
        Guid? createdByUserId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet
            .Include(d => d.ColorFabric)
            .Include(d => d.Order)
                .ThenInclude(o => o!.Customer)
            .Include(d => d.CreatedByUser)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(d =>
                d.DesignName.ToLower().Contains(search) ||
                (d.Designer != null && d.Designer.ToLower().Contains(search)) ||
                (d.CustomerFullName != null && d.CustomerFullName.ToLower().Contains(search)) ||
                (d.SaleStaff != null && d.SaleStaff.ToLower().Contains(search)));
        }

        if (orderId.HasValue)
            query = query.Where(d => d.OrderId == orderId.Value);

        if (colorFabricId.HasValue)
            query = query.Where(d => d.ColorFabricId == colorFabricId.Value);

        if (createdByUserId.HasValue)
            query = query.Where(d => d.CreatedByUserId == createdByUserId.Value);

        if (fromDate.HasValue)
            query = query.Where(d => d.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(d => d.CreatedAt <= toDate.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "designname" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.DesignName)
                : query.OrderByDescending(d => d.DesignName),
            "designer" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.Designer)
                : query.OrderByDescending(d => d.Designer),
            "finisheddate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.FinishedDate)
                : query.OrderByDescending(d => d.FinishedDate),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
