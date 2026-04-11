using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Data;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DealStage> DealStages => Set<DealStage>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ColorFabric> ColorFabrics => Set<ColorFabric>();
    public DbSet<ShirtComponent> ShirtComponents => Set<ShirtComponent>();
    public DbSet<Design> Designs => Set<Design>();
    public DbSet<ProductionStage> ProductionStages => Set<ProductionStage>();
    public DbSet<OrderProductionStep> OrderProductionSteps => Set<OrderProductionStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
