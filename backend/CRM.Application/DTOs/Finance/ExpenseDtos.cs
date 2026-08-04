namespace CRM.Application.DTOs.Finance;

// ── Đầu mục chi phí cố định ───────────────────────────────────────────────
public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    /// <summary>Số dòng chi phí đang dùng đầu mục này — dùng để chặn xóa ở UI.</summary>
    public int UsageCount { get; set; }
}

public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateExpenseCategoryDto : CreateExpenseCategoryDto
{
    public Guid Id { get; set; }
}

// ── Chi phí cố định (theo ngày) ───────────────────────────────────────────
public class FixedExpenseDto
{
    public Guid Id { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFixedExpenseDto
{
    public DateOnly ExpenseDate { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
}

public class UpdateFixedExpenseDto : CreateFixedExpenseDto
{
    public Guid Id { get; set; }
}

public class FixedExpenseFilterDto
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class ExpenseCategoryTotalDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class FixedExpenseListResultDto
{
    public List<FixedExpenseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public decimal GrandTotal { get; set; }
    public List<ExpenseCategoryTotalDto> TotalsByCategory { get; set; } = new();
}

// ── Chi phí nhân sự (theo tháng) ──────────────────────────────────────────
public class PayrollEntryDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public decimal Salary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Insurance { get; set; }
    public decimal OtherCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

public class CreatePayrollEntryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public decimal Salary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Insurance { get; set; }
    public decimal OtherCost { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePayrollEntryDto : CreatePayrollEntryDto
{
    public Guid Id { get; set; }
}

public class PayrollPeriodDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<PayrollEntryDto> Items { get; set; } = new();
    public decimal TotalSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalInsurance { get; set; }
    public decimal TotalOther { get; set; }
    public decimal GrandTotal { get; set; }
}
