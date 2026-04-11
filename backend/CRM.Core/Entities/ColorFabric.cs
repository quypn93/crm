namespace CRM.Core.Entities;

public class ColorFabric : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<ShirtComponent> ShirtComponents { get; set; } = new List<ShirtComponent>();
    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();
}
