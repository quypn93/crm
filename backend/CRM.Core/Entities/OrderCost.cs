namespace CRM.Core.Entities;

/// <summary>
/// Chi phí trực tiếp gắn vào 1 đơn hàng (giá vốn hàng hóa). Kế toán nhập tay hoặc import file.
/// 1 đơn ↔ 1 bản ghi. Dữ liệu nhạy cảm — chỉ Admin/Kế toán được đọc.
/// </summary>
public class OrderCost : BaseEntity
{
    public Guid OrderId { get; set; }

    public decimal CostAmount { get; set; }             // Giá cost (giá vốn hàng hóa)
    public decimal ShippingCost { get; set; }           // Chi phí ship hàng
    public decimal OutboundShippingCost { get; set; }   // Chi phí gửi hàng đi
    public decimal OtherCost { get; set; }              // Chi phí phát sinh khác của đơn

    /// <summary>Tổng 4 khoản trên — ghi sẵn khi lưu để báo cáo khỏi tính lại.</summary>
    public decimal TotalCost { get; set; }

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

    /// <summary>Tính lại TotalCost từ 4 khoản thành phần.</summary>
    public void RecalculateTotal()
    {
        TotalCost = CostAmount + ShippingCost + OutboundShippingCost + OtherCost;
    }
}
