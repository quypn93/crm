using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class NotificationRolePreferenceRepository : Repository<NotificationRolePreference>, INotificationRolePreferenceRepository
{
    public NotificationRolePreferenceRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NotificationRolePreference>> GetAllAsync(IEnumerable<Guid>? roleIds = null)
    {
        var query = _dbSet.Include(p => p.Role).AsQueryable();
        if (roleIds != null)
        {
            var ids = roleIds.ToList();
            query = query.Where(p => ids.Contains(p.RoleId));
        }
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<NotificationRolePreference>> GetByRoleIdsAsync(IEnumerable<Guid> roleIds)
    {
        var ids = roleIds.ToList();
        if (ids.Count == 0) return Enumerable.Empty<NotificationRolePreference>();

        return await _dbSet
            .Where(p => ids.Contains(p.RoleId))
            .ToListAsync();
    }

    public Task<NotificationRolePreference?> GetAsync(Guid roleId, NotificationType type)
    {
        return _dbSet.FirstOrDefaultAsync(p => p.RoleId == roleId && p.Type == type);
    }
}
