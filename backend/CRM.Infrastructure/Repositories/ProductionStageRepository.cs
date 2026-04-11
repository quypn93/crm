using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ProductionStageRepository : Repository<ProductionStage>, IProductionStageRepository
{
    public ProductionStageRepository(CrmDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductionStage>> GetActiveStagesOrderedAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.StageOrder)
            .ToListAsync();
    }
}
