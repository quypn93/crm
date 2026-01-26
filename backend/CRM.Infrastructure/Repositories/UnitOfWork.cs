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
    private ICustomerRepository? _customers;
    private IDealRepository? _deals;
    private ITaskRepository? _tasks;

    public UnitOfWork(CrmDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IDealRepository Deals => _deals ??= new DealRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);

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
