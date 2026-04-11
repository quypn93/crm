namespace CRM.Core.Entities;

public class ProductionStage : BaseEntity
{
    public int StageOrder { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ResponsibleRole { get; set; }   // matches RoleNames constant, null = any role
    public bool IsActive { get; set; } = true;

    public virtual ICollection<OrderProductionStep> Steps { get; set; } = new List<OrderProductionStep>();
}
