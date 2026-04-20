using CRM.Core.Entities;
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
    private IRepository<Collection>? _collections;
    private IRepository<Material>? _materials;
    private IRepository<ProductForm>? _productForms;
    private IRepository<ProductSpecification>? _productSpecs;
    private IRepository<ProductionDaysOption>? _productionDaysOptions;
    private IRepository<DepositTransaction>? _depositTransactions;

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
    public IRepository<Collection> Collections => _collections ??= new Repository<Collection>(_context);
    public IRepository<Material> Materials => _materials ??= new Repository<Material>(_context);
    public IRepository<ProductForm> ProductForms => _productForms ??= new Repository<ProductForm>(_context);
    public IRepository<ProductSpecification> ProductSpecifications => _productSpecs ??= new Repository<ProductSpecification>(_context);
    public IRepository<ProductionDaysOption> ProductionDaysOptions => _productionDaysOptions ??= new Repository<ProductionDaysOption>(_context);
    public IRepository<DepositTransaction> DepositTransactions => _depositTransactions ??= new Repository<DepositTransaction>(_context);

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
