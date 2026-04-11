namespace CRM.Core.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    // Product info
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? Description { get; set; }

    // Specifications (for uniforms)
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }

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
}
