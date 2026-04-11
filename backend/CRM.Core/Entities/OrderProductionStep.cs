namespace CRM.Core.Entities;

public class OrderProductionStep : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductionStageId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual ProductionStage ProductionStage { get; set; } = null!;
    public virtual User? CompletedByUser { get; set; }
}
