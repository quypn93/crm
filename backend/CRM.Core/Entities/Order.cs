using CRM.Core.Enums;

namespace CRM.Core.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }   // free-text khi không chọn từ danh sách
    public Guid? DealId { get; set; }

    // Order details
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; } = 10; // VAT 10%
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";

    // Dates
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }   // Ngày yêu cầu giao
    public DateTime? CompletionDate { get; set; }         // Ngày xong (hoàn thành sản xuất)
    public DateTime? ReturnDate { get; set; }             // Ngày trả hàng cho KH
    public DateTime? ActualDeliveryDate { get; set; }

    // Shipping info
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingNotes { get; set; }

    // Payment info
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Uniform-specific fields
    public string? StyleNotes { get; set; }         // Kiểu dáng: form, quy cách, bộ sưu tập, cổ dệt...
    public string? PersonNamesBySize { get; set; }  // JSON: {"M":["Nguyễn A","Trần B"],"L":["Lê C"]}
    public string? GiftItems { get; set; }          // JSON: [{"description":"Cờ 1M x 1.5M"},{"description":"Sticker"}]

    // QR & Production
    public string? QrCodeToken { get; set; }          // 22-char URL-safe Base64 of OrderId
    public string? QrCodeImageBase64 { get; set; }    // PNG base64 for embedding in template

    // Foreign keys
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? DesignerUserId { get; set; }         // người thiết kế được giao

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Deal? Deal { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual User? DesignerUser { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();
    public virtual ICollection<OrderProductionStep> ProductionSteps { get; set; } = new List<OrderProductionStep>();
}
