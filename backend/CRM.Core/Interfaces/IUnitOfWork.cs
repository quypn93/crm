using CRM.Core.Interfaces.Repositories;

namespace CRM.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    ICustomerRepository Customers { get; }
    IDealRepository Deals { get; }
    ITaskRepository Tasks { get; }
    IActivityLogRepository ActivityLogs { get; }
    IOrderRepository Orders { get; }
    IColorFabricRepository ColorFabrics { get; }
    IShirtComponentRepository ShirtComponents { get; }
    IDesignRepository Designs { get; }
    IProductionStageRepository ProductionStages { get; }
    IOrderProductionStepRepository OrderProductionSteps { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
