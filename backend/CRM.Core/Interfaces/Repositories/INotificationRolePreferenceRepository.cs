using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.Core.Interfaces.Repositories;

public interface INotificationRolePreferenceRepository : IRepository<NotificationRolePreference>
{
    Task<IEnumerable<NotificationRolePreference>> GetAllAsync(IEnumerable<Guid>? roleIds = null);

    Task<IEnumerable<NotificationRolePreference>> GetByRoleIdsAsync(IEnumerable<Guid> roleIds);

    Task<NotificationRolePreference?> GetAsync(Guid roleId, NotificationType type);
}
