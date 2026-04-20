namespace CRM.Core.Entities;

public class Collection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<CollectionMaterial> Materials { get; set; } = new List<CollectionMaterial>();
    public virtual ICollection<CollectionColor> Colors { get; set; } = new List<CollectionColor>();
    public virtual ICollection<CollectionForm> Forms { get; set; } = new List<CollectionForm>();
    public virtual ICollection<CollectionSpecification> Specifications { get; set; } = new List<CollectionSpecification>();
}
