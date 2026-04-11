namespace CRM.Core.Entities;

public class ProductAttribute : BaseEntity
{
    /// <summary>Loại thuộc tính: "Color" hoặc "Material"</summary>
    public string Type { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}
