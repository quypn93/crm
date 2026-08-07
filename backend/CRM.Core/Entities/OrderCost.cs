namespace CRM.Core.Entities;

/// <summary>
/// Chi phí trực tiếp gắn vào 1 đơn hàng (giá vốn hàng hóa). Kế toán nhập tay hoặc import file.
/// 1 đơn ↔ 1 bản ghi. Dữ liệu nhạy cảm — chỉ Admin/Kế toán được đọc.
/// </summary>
public class OrderCost : BaseEntity
{
    public Guid OrderId { get; set; }

    /// <summary>Đơn giá cost của 1 sản phẩm. Kế toán nhập ô này, KHÔNG nhập tổng.</summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Tổng số lượng sản phẩm của đơn tại thời điểm lưu — snapshot của SUM(OrderItems.Quantity).
    /// Đơn hàng đồng phục tách nhiều dòng theo size nhưng chỉ 1 loại SP, 1 mức giá.
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>Thành tiền giá vốn = UnitCost × TotalQuantity.</summary>
    public decimal CostAmount { get; set; }

    /// <summary>Đơn giá 1 phần quà tặng.</summary>
    public decimal GiftUnitCost { get; set; }

    /// <summary>Số lượng quà tặng — nhập tay, KHÁC số lượng áo (tặng 1 cờ cho đơn 200 áo).</summary>
    public int GiftQuantity { get; set; }

    /// <summary>Thành tiền quà tặng = GiftUnitCost × GiftQuantity.</summary>
    public decimal GiftAmount { get; set; }

    public decimal ShippingCost { get; set; }           // Chi phí ship hàng
    public decimal OutboundShippingCost { get; set; }   // Chi phí gửi hàng đi
    public decimal OtherCost { get; set; }              // Chi phí phát sinh khác của đơn

    /// <summary>Tổng chi phí của đơn — ghi sẵn khi lưu để báo cáo khỏi tính lại.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Mã giao hàng kế toán nhập tay — dùng cho đơn gửi ngoài hệ thống.
    /// Đơn tạo vận đơn qua API thì đọc GhtkLabel/ViettelPostLabel trên Order, không ghi vào đây.
    /// </summary>
    public string? ShippingCode { get; set; }

    /// <summary>
    /// Số tiền thanh toán kế toán nhập để đối soát. ĐỘC LẬP với Order.PaidAmount —
    /// PaidAmount đang chi phối số tiền GHTK/Viettel Post thu hộ khách (TotalAmount − PaidAmount),
    /// ghi đè vào đó sẽ làm hãng vận chuyển thu sai tiền.
    /// </summary>
    public decimal SettlementAmount { get; set; }

    // File giá cost kế toán đính kèm cho đơn (tham chiếu, không parse)
    public string? CostFileUrl { get; set; }
    public string? CostFileName { get; set; }

    public string? Notes { get; set; }

    /// <summary>Đã chốt sổ — kế toán không sửa được nữa, chỉ Admin mở khóa.</summary>
    public bool IsFinalized { get; set; }

    public Guid? EnteredByUserId { get; set; }
    public DateTime? EnteredAt { get; set; }

    // Navigation
    public virtual Order Order { get; set; } = null!;
    public virtual User? EnteredByUser { get; set; }

    /// <summary>
    /// Tính lại thành tiền và tổng chi phí. Truyền tổng số lượng hiện tại của đơn;
    /// bỏ trống thì giữ nguyên TotalQuantity đã lưu.
    /// </summary>
    public void Recalculate(int? totalQuantity = null)
    {
        if (totalQuantity.HasValue) TotalQuantity = totalQuantity.Value;

        CostAmount = UnitCost * TotalQuantity;
        GiftAmount = GiftUnitCost * GiftQuantity;
        TotalCost = CostAmount + GiftAmount + ShippingCost + OutboundShippingCost + OtherCost;
    }
}
