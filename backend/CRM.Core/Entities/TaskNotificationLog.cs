using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class TaskNotificationLog : BaseEntity
{
    public Guid TaskId { get; set; }
    public NotificationType Type { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public virtual TaskItem Task { get; set; } = null!;
}
