namespace CRM.Core.Entities;

/// <summary>
/// Đầu mục chi phí cố định (tiền nhà, điện nước, internet...). Kế toán tự thêm khi thiếu.
/// Mục IsSystem là mục seed sẵn — chỉ được ẩn (IsActive=false), không xóa.
/// </summary>
public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }

    public virtual ICollection<FixedExpense> Expenses { get; set; } = new List<FixedExpense>();
}
