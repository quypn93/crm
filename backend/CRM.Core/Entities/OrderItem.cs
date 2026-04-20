namespace CRM.Core.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    // Collection (thay thế ProductName)
    public Guid? CollectionId { get; set; }
    public string? CollectionName { get; set; }         // snapshot tên BST lúc đặt
    public string? ProductCode { get; set; }
    public string? Description { get; set; }

    // Specifications (for uniforms) - lấy từ pool chung, filter theo Collection
    public string? Size { get; set; }
    public Guid? MainColorId { get; set; }              // ColorFabric
    public Guid? AccentColorId { get; set; }            // ColorFabric
    public Guid? MaterialId { get; set; }
    public Guid? FormId { get; set; }
    public Guid? SpecificationId { get; set; }
    public string? MainColorName { get; set; }
    public string? AccentColorName { get; set; }
    public string? MaterialName { get; set; }
    public string? FormName { get; set; }
    public string? SpecificationName { get; set; }

    // Quantity and pricing
    public int Quantity { get; set; }
    public string Unit { get; set; } = "cái"; // cái, bộ, chiếc
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Collection? Collection { get; set; }
}
