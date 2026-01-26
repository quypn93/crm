using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? CompletedAt { get; set; }

    // Foreign keys
    public Guid? CustomerId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual Deal? Deal { get; set; }
    public virtual User? AssignedToUser { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;
}
