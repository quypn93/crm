using CRM.Core.Entities;
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
    IProvinceRepository Provinces { get; }
    IWardRepository Wards { get; }

    // Lookup pools + admin-managed tables
    IRepository<Collection> Collections { get; }
    IRepository<Material> Materials { get; }
    IRepository<ProductForm> ProductForms { get; }
    IRepository<ProductSpecification> ProductSpecifications { get; }
    IRepository<ProductionDaysOption> ProductionDaysOptions { get; }
    IRepository<DepositTransaction> DepositTransactions { get; }

    INotificationRepository Notifications { get; }
    INotificationRolePreferenceRepository NotificationRolePreferences { get; }
    ITaskNotificationLogRepository TaskNotificationLogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
