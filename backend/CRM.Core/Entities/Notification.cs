using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class Notification : BaseEntity
{
    public Guid RecipientUserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }

    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual User RecipientUser { get; set; } = null!;
}
