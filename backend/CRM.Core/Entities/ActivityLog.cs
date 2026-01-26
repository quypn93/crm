namespace CRM.Core.Entities;

public class ActivityLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }

    // Foreign keys
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

public static class ActivityActions
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
    public const string StageChanged = "StageChanged";
    public const string StatusChanged = "StatusChanged";
    public const string Assigned = "Assigned";
}
