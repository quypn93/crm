using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class DealRepository : Repository<Deal>, IDealRepository
{
    public DealRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Deal?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.Customer)
            .Include(d => d.Stage)
            .Include(d => d.CreatedByUser)
            .Include(d => d.AssignedToUser)
            .Include(d => d.Tasks)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<(IEnumerable<Deal> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? stageId,
        Guid? customerId,
        Guid? assignedTo,
        decimal? minValue,
        decimal? maxValue,
        DateTime? closeDateFrom,
        DateTime? closeDateTo,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder)
    {
        var query = _dbSet
            .Include(d => d.Customer)
            .Include(d => d.Stage)
            .Include(d => d.CreatedByUser)
            .Include(d => d.AssignedToUser)
            .Include(d => d.Tasks)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(search) ||
                (d.Customer != null && d.Customer.Name.ToLower().Contains(search)));
        }

        if (stageId.HasValue)
            query = query.Where(d => d.StageId == stageId.Value);

        if (customerId.HasValue)
            query = query.Where(d => d.CustomerId == customerId.Value);

        if (assignedTo.HasValue)
            query = query.Where(d => d.AssignedToUserId == assignedTo.Value);

        if (minValue.HasValue)
            query = query.Where(d => d.Value >= minValue.Value);

        if (maxValue.HasValue)
            query = query.Where(d => d.Value <= maxValue.Value);

        if (closeDateFrom.HasValue)
            query = query.Where(d => d.ExpectedCloseDate >= closeDateFrom.Value);

        if (closeDateTo.HasValue)
            query = query.Where(d => d.ExpectedCloseDate <= closeDateTo.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.Title)
                : query.OrderByDescending(d => d.Title),
            "value" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.Value)
                : query.OrderByDescending(d => d.Value),
            "expectedclosedate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(d => d.ExpectedCloseDate)
                : query.OrderByDescending(d => d.ExpectedCloseDate),
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

    public async Task<IEnumerable<Deal>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet
            .Include(d => d.Stage)
            .Where(d => d.CustomerId == customerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deal>> GetByStageAsync(Guid stageId)
    {
        return await _dbSet
            .Include(d => d.Customer)
            .Include(d => d.AssignedToUser)
            .Where(d => d.StageId == stageId)
            .OrderBy(d => d.ExpectedCloseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DealStage>> GetAllStagesAsync()
    {
        return await _context.DealStages
            .OrderBy(s => s.Order)
            .ToListAsync();
    }

    public async Task<DealStage?> GetDefaultStageAsync()
    {
        return await _context.DealStages.FirstOrDefaultAsync(s => s.IsDefault);
    }

    public async Task<DealStage?> GetWonStageAsync()
    {
        return await _context.DealStages.FirstOrDefaultAsync(s => s.IsWonStage);
    }

    public async Task<DealStage?> GetLostStageAsync()
    {
        return await _context.DealStages.FirstOrDefaultAsync(s => s.IsLostStage);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? from, DateTime? to)
    {
        var query = _dbSet.Where(d => d.Stage.IsWonStage);

        if (from.HasValue)
            query = query.Where(d => d.ActualCloseDate >= from.Value);

        if (to.HasValue)
            query = query.Where(d => d.ActualCloseDate <= to.Value);

        return await query.SumAsync(d => d.Value);
    }

    public async Task<IEnumerable<(DealStage Stage, int Count, decimal TotalValue)>> GetDealsByStageAsync()
    {
        var stages = await _context.DealStages
            .Include(s => s.Deals)
            .OrderBy(s => s.Order)
            .ToListAsync();

        return stages.Select(s => (
            Stage: s,
            Count: s.Deals.Count,
            TotalValue: s.Deals.Sum(d => d.Value)
        ));
    }
}
