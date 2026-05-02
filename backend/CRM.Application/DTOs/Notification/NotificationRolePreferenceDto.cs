using CRM.Core.Enums;

namespace CRM.Application.DTOs.Notification;

public class NotificationRolePreferenceDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool InApp { get; set; }
    public bool Email { get; set; }
    public bool IsDefault { get; set; } // true nếu fallback từ config (chưa override trong DB)
}

public class UpdateRolePreferencesRequest
{
    public List<RolePreferenceItem> Items { get; set; } = new();
}

public class RolePreferenceItem
{
    public Guid RoleId { get; set; }
    public NotificationType Type { get; set; }
    public bool InApp { get; set; }
    public bool Email { get; set; }
}
