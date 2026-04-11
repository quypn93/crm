using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class ShirtComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? WomenImageUrl { get; set; }
    public ComponentType Type { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Foreign keys
    public Guid? ColorFabricId { get; set; }

    // Navigation properties
    public virtual ColorFabric? ColorFabric { get; set; }
}
