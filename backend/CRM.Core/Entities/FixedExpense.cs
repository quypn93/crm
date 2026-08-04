namespace CRM.Core.Entities;

/// <summary>
/// Chi phí cố định — nhập theo NGÀY. 1 dòng = 1 ngày + 1 đầu mục + 1 số tiền.
/// </summary>
public class FixedExpense : BaseEntity
{
    /// <summary>Ngày phát sinh chi phí (date thuần, không giờ → tránh lệch múi giờ khi gom tháng).</summary>
    public DateOnly ExpenseDate { get; set; }

    public Guid ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;   // snapshot phòng khi đầu mục đổi tên

    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }                 // ảnh hóa đơn / chứng từ
    public string? AttachmentName { get; set; }

    public Guid? CreatedByUserId { get; set; }

    // Navigation
    public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
