using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class OrderProductionStepRepository : Repository<OrderProductionStep>, IOrderProductionStepRepository
{
    public OrderProductionStepRepository(CrmDbContext context) : base(context) { }

    public async Task<IEnumerable<OrderProductionStep>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(s => s.ProductionStage)
            .Include(s => s.CompletedByUser)
            .Where(s => s.OrderId == orderId)
            .OrderBy(s => s.ProductionStage.StageOrder)
            .ToListAsync();
    }

    public async Task<OrderProductionStep?> GetByOrderAndStageAsync(Guid orderId, Guid stageId)
    {
        return await _dbSet
            .Include(s => s.ProductionStage)
            .Include(s => s.CompletedByUser)
            .FirstOrDefaultAsync(s => s.OrderId == orderId && s.ProductionStageId == stageId);
    }

    public async Task<bool> AreAllStepsCompletedAsync(Guid orderId)
    {
        var total = await _dbSet.CountAsync(s => s.OrderId == orderId);
        if (total == 0) return false;
        var completed = await _dbSet.CountAsync(s => s.OrderId == orderId && s.IsCompleted);
        return total == completed;
    }

    public async Task InitializeStepsForOrderAsync(Guid orderId, IEnumerable<ProductionStage> stages)
    {
        foreach (var stage in stages)
        {
            var exists = await _dbSet.AnyAsync(s => s.OrderId == orderId && s.ProductionStageId == stage.Id);
            if (!exists)
            {
                await _dbSet.AddAsync(new OrderProductionStep
                {
                    OrderId = orderId,
                    ProductionStageId = stage.Id,
                    IsCompleted = false
                });
            }
        }
    }
}
