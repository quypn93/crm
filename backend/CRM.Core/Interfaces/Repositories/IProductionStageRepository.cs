using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IProductionStageRepository : IRepository<ProductionStage>
{
    Task<IEnumerable<ProductionStage>> GetActiveStagesOrderedAsync();
}
