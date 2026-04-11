using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IOrderProductionStepRepository : IRepository<OrderProductionStep>
{
    Task<IEnumerable<OrderProductionStep>> GetByOrderIdAsync(Guid orderId);
    Task<OrderProductionStep?> GetByOrderAndStageAsync(Guid orderId, Guid stageId);
    Task<bool> AreAllStepsCompletedAsync(Guid orderId);
    Task InitializeStepsForOrderAsync(Guid orderId, IEnumerable<ProductionStage> stages);
}
