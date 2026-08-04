namespace CRM.Core.Entities;

/// <summary>
/// Chi phí nhân sự — nhập theo THÁNG. Mỗi dòng = 1 nhân sự trong 1 kỳ lương.
/// </summary>
public class PayrollEntry : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }               // 1..12

    /// <summary>Null nếu nhân sự không có tài khoản trên hệ thống.</summary>
    public Guid? UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;   // snapshot tên, luôn có
    public string? Position { get; set; }                      // chức danh / bộ phận

    public decimal Salary { get; set; }          // Lương
    public decimal Allowance { get; set; }       // Phụ cấp
    public decimal Insurance { get; set; }       // Bảo hiểm
    public decimal OtherCost { get; set; }       // Chi phí khác

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Navigation
    public virtual User? User { get; set; }

    public void RecalculateTotal()
    {
        TotalAmount = Salary + Allowance + Insurance + OtherCost;
    }
}
