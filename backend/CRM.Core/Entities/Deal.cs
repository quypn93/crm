namespace CRM.Core.Entities;

public class Deal : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Value { get; set; } = 0;
    public string Currency { get; set; } = "VND";
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public int Probability { get; set; } = 0;
    public string? Notes { get; set; }
    public string? LostReason { get; set; }

    // Foreign keys
    public Guid CustomerId { get; set; }
    public Guid StageId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual DealStage Stage { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
