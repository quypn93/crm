namespace CRM.Core.Entities;

public class CollectionMaterial
{
    public Guid CollectionId { get; set; }
    public Guid MaterialId { get; set; }
    public virtual Collection Collection { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
}

public class CollectionColor
{
    public Guid CollectionId { get; set; }
    public Guid ColorFabricId { get; set; }
    public virtual Collection Collection { get; set; } = null!;
    public virtual ColorFabric ColorFabric { get; set; } = null!;
}

public class CollectionForm
{
    public Guid CollectionId { get; set; }
    public Guid ProductFormId { get; set; }
    public virtual Collection Collection { get; set; } = null!;
    public virtual ProductForm ProductForm { get; set; } = null!;
}

public class CollectionSpecification
{
    public Guid CollectionId { get; set; }
    public Guid ProductSpecificationId { get; set; }
    public virtual Collection Collection { get; set; } = null!;
    public virtual ProductSpecification ProductSpecification { get; set; } = null!;
}
