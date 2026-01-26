using CRM.Core.Interfaces.Repositories;

namespace CRM.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICustomerRepository Customers { get; }
    IDealRepository Deals { get; }
    ITaskRepository Tasks { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
