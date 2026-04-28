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

    // Shipping info — cấu trúc 2 cấp (Tỉnh + Xã), bỏ Huyện từ 07/2025
    public DeliveryMethod? DeliveryMethod { get; set; }     // Hình thức giao: nhà giao / giao xe / GHTK
    public string? ShippingContactName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingAddress { get; set; }            // Số nhà, đường
    public string? ShippingProvinceCode { get; set; }
    public string? ShippingProvinceName { get; set; }
    public string? ShippingWardCode { get; set; }
    public string? ShippingWardName { get; set; }
    public string? ShippingCity { get; set; }               // [Deprecated] giữ lại để tương thích dữ liệu cũ
    public string? ShippingNotes { get; set; }

    // GHTK (Giao Hàng Tiết Kiệm) tracking — chỉ điền khi DeliveryMethod = GHTK
    public string? GhtkLabel { get; set; }              // Mã vận đơn trả về từ GHTK
    public string? GhtkTrackingUrl { get; set; }
    public string? GhtkStatus { get; set; }             // Trạng thái raw từ GHTK (ready_to_pick, picking, delivered...)
    public int? GhtkStatusCode { get; set; }            // Status code (1-45 theo docs GHTK)
    public decimal? GhtkFee { get; set; }               // Phí vận chuyển GHTK trả (reference, không ghi đè ShippingFee)
    public decimal? GhtkInsuranceFee { get; set; }
    public string? GhtkLastError { get; set; }          // Lỗi lần gọi API gần nhất
    public DateTime? GhtkSyncedAt { get; set; }

    // Payment info
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Uniform-specific fields
    public string? StyleNotes { get; set; }

    // Production days option (admin-managed)
    public Guid? ProductionDaysOptionId { get; set; }
    public int? ProductionDays { get; set; }          // Snapshot số ngày sản xuất

    // Deposit code (sale nhập mã cọc tiền - liên kết với DepositTransaction)
    public string? DepositCode { get; set; }

    // Designer upload (ảnh đơn hàng sau khi thiết kế)
    public string? DesignImageUrl { get; set; }

    // Design có sẵn (tái sử dụng từ flow assignment của Designer)
    public Guid? DesignId { get; set; }

    // QR & Production
    public string? QrCodeToken { get; set; }
    public string? QrCodeImageBase64 { get; set; }

    // Foreign keys
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? DesignerUserId { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Deal? Deal { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual User? DesignerUser { get; set; }
    public virtual ProductionDaysOption? ProductionDaysOption { get; set; }
    public virtual Design? Design { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();
    public virtual ICollection<OrderProductionStep> ProductionSteps { get; set; } = new List<OrderProductionStep>();
}
