using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CrmDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private ICustomerRepository? _customers;
    private IDealRepository? _deals;
    private ITaskRepository? _tasks;
    private IActivityLogRepository? _activityLogs;
    private IOrderRepository? _orders;
    private IColorFabricRepository? _colorFabrics;
    private IShirtComponentRepository? _shirtComponents;
    private IDesignRepository? _designs;
    private IProductionStageRepository? _productionStages;
    private IOrderProductionStepRepository? _orderProductionSteps;

    public UnitOfWork(CrmDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IDealRepository Deals => _deals ??= new DealRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IColorFabricRepository ColorFabrics => _colorFabrics ??= new ColorFabricRepository(_context);
    public IShirtComponentRepository ShirtComponents => _shirtComponents ??= new ShirtComponentRepository(_context);
    public IDesignRepository Designs => _designs ??= new DesignRepository(_context);
    public IProductionStageRepository ProductionStages => _productionStages ??= new ProductionStageRepository(_context);
    public IOrderProductionStepRepository OrderProductionSteps => _orderProductionSteps ??= new OrderProductionStepRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
