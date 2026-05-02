using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class NotificationRolePreference : BaseEntity
{
    public Guid RoleId { get; set; }
    public NotificationType Type { get; set; }
    public bool InApp { get; set; } = true;
    public bool Email { get; set; }

    public virtual Role Role { get; set; } = null!;
}
