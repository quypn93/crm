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
    public Guid? AccentColorId { get; set; }            // AccentColor — Màu phối 1 (chọn tự do)
    public Guid? AccentColor2Id { get; set; }           // AccentColor — Màu phối 2 (chọn tự do)
    public Guid? CollarId { get; set; }                 // Collar — Bo cổ
    // Màu bo cổ — số lượng slot hiển thị ở FE phụ thuộc Collar.ColorCount (1..3).
    public Guid? CollarColor1Id { get; set; }            // ColorFabric — Màu bo cổ chính, ăn theo chất liệu (giống MainColorId)
    public Guid? CollarColor2Id { get; set; }            // AccentColor — Màu bo cổ phối 1 (chọn tự do)
    public Guid? CollarColor3Id { get; set; }            // AccentColor — Màu bo cổ phối 2 (chọn tự do)
    public Guid? MaterialId { get; set; }
    public Guid? FormId { get; set; }
    public Guid? SpecificationId { get; set; }
    public string? MainColorName { get; set; }
    public string? AccentColorName { get; set; }
    public string? AccentColor2Name { get; set; }
    public string? CollarName { get; set; }
    public string? CollarColor1Name { get; set; }
    public string? CollarColor2Name { get; set; }
    public string? CollarColor3Name { get; set; }
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
