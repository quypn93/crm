using CRM.Application.DTOs.Finance;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services.Finance;

// ── Đầu mục chi phí cố định ───────────────────────────────────────────────
public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly CrmDbContext _db;
    public ExpenseCategoryService(CrmDbContext db) { _db = db; }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(bool activeOnly = false)
    {
        var q = _db.ExpenseCategories.AsNoTracking();
        if (activeOnly) q = q.Where(c => c.IsActive);

        return await q
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                IsSystem = c.IsSystem,
                UsageCount = c.Expenses.Count
            })
            .ToListAsync();
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto)
    {
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Tên đầu mục không được để trống.");
        if (await _db.ExpenseCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
            throw new InvalidOperationException($"Đầu mục \"{name}\" đã tồn tại.");

        var e = new ExpenseCategory
        {
            Name = name,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            IsSystem = false
        };
        _db.ExpenseCategories.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e, 0);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(UpdateExpenseCategoryDto dto)
    {
        var e = await _db.ExpenseCategories.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy đầu mục chi phí.");

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Tên đầu mục không được để trống.");
        if (await _db.ExpenseCategories.AnyAsync(c => c.Id != dto.Id && c.Name.ToLower() == name.ToLower()))
            throw new InvalidOperationException($"Đầu mục \"{name}\" đã tồn tại.");

        e.Name = name;
        e.Description = dto.Description;
        e.SortOrder = dto.SortOrder;
        e.IsActive = dto.IsActive;
        e.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        var usage = await _db.FixedExpenses.CountAsync(x => x.ExpenseCategoryId == e.Id);
        return ToDto(e, usage);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.ExpenseCategories.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy đầu mục chi phí.");

        if (e.IsSystem)
            throw new InvalidOperationException("Đầu mục mặc định không xóa được — hãy bỏ tick \"Hoạt động\" để ẩn.");
        if (await _db.FixedExpenses.AnyAsync(x => x.ExpenseCategoryId == id))
            throw new InvalidOperationException("Đầu mục đã phát sinh chi phí — hãy bỏ tick \"Hoạt động\" để ẩn thay vì xóa.");

        _db.ExpenseCategories.Remove(e);
        await _db.SaveChangesAsync();
    }

    private static ExpenseCategoryDto ToDto(ExpenseCategory e, int usage) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        SortOrder = e.SortOrder,
        IsActive = e.IsActive,
        IsSystem = e.IsSystem,
        UsageCount = usage
    };
}

// ── Chi phí cố định (nhập theo ngày) ──────────────────────────────────────
public class FixedExpenseService : IFixedExpenseService
{
    private readonly CrmDbContext _db;
    public FixedExpenseService(CrmDbContext db) { _db = db; }

    public async Task<FixedExpenseListResultDto> GetListAsync(FixedExpenseFilterDto filter)
    {
        var q = _db.FixedExpenses.AsNoTracking();

        if (filter.DateFrom.HasValue) q = q.Where(x => x.ExpenseDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) q = q.Where(x => x.ExpenseDate <= filter.DateTo.Value);
        if (filter.CategoryId.HasValue) q = q.Where(x => x.ExpenseCategoryId == filter.CategoryId.Value);

        var totalCount = await q.CountAsync();
        var grandTotal = totalCount == 0 ? 0m : await q.SumAsync(x => x.Amount);

        // Sắp xếp ở client — tập kết quả chỉ bằng số đầu mục, và tránh ORDER BY trên cột decimal
        // (SQLite dùng trong test không hỗ trợ).
        var byCategory = (await q
                .GroupBy(x => new { x.ExpenseCategoryId, x.CategoryName })
                .Select(g => new ExpenseCategoryTotalDto
                {
                    CategoryId = g.Key.ExpenseCategoryId,
                    CategoryName = g.Key.CategoryName,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync())
            .OrderByDescending(x => x.Amount)
            .ToList();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 100 : filter.PageSize;

        var items = await q
            .OrderByDescending(x => x.ExpenseDate).ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FixedExpenseDto
            {
                Id = x.Id,
                ExpenseDate = x.ExpenseDate,
                ExpenseCategoryId = x.ExpenseCategoryId,
                CategoryName = x.CategoryName,
                Amount = x.Amount,
                Notes = x.Notes,
                AttachmentUrl = x.AttachmentUrl,
                AttachmentName = x.AttachmentName,
                CreatedByUserName = x.CreatedByUser != null
                    ? (x.CreatedByUser.FirstName + " " + x.CreatedByUser.LastName).Trim()
                    : null,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new FixedExpenseListResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            GrandTotal = grandTotal,
            TotalsByCategory = byCategory
        };
    }

    public async Task<FixedExpenseDto> CreateAsync(CreateFixedExpenseDto dto, Guid userId)
    {
        var category = await _db.ExpenseCategories.FindAsync(dto.ExpenseCategoryId)
            ?? throw new InvalidOperationException("Đầu mục chi phí không tồn tại.");
        if (dto.Amount < 0) throw new InvalidOperationException("Số tiền không được âm.");

        var e = new FixedExpense
        {
            ExpenseDate = dto.ExpenseDate == default ? DateOnly.FromDateTime(FinanceHelpers.ToVn(DateTime.UtcNow)) : dto.ExpenseDate,
            ExpenseCategoryId = category.Id,
            CategoryName = category.Name,
            Amount = dto.Amount,
            Notes = dto.Notes,
            AttachmentUrl = dto.AttachmentUrl,
            AttachmentName = dto.AttachmentName,
            CreatedByUserId = userId
        };
        _db.FixedExpenses.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<FixedExpenseDto> UpdateAsync(UpdateFixedExpenseDto dto)
    {
        var e = await _db.FixedExpenses.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy khoản chi phí.");
        if (dto.Amount < 0) throw new InvalidOperationException("Số tiền không được âm.");

        if (e.ExpenseCategoryId != dto.ExpenseCategoryId)
        {
            var category = await _db.ExpenseCategories.FindAsync(dto.ExpenseCategoryId)
                ?? throw new InvalidOperationException("Đầu mục chi phí không tồn tại.");
            e.ExpenseCategoryId = category.Id;
            e.CategoryName = category.Name;
        }

        e.ExpenseDate = dto.ExpenseDate;
        e.Amount = dto.Amount;
        e.Notes = dto.Notes;
        e.AttachmentUrl = dto.AttachmentUrl;
        e.AttachmentName = dto.AttachmentName;
        e.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.FixedExpenses.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy khoản chi phí.");
        _db.FixedExpenses.Remove(e);
        await _db.SaveChangesAsync();
    }

    private static FixedExpenseDto ToDto(FixedExpense e) => new()
    {
        Id = e.Id,
        ExpenseDate = e.ExpenseDate,
        ExpenseCategoryId = e.ExpenseCategoryId,
        CategoryName = e.CategoryName,
        Amount = e.Amount,
        Notes = e.Notes,
        AttachmentUrl = e.AttachmentUrl,
        AttachmentName = e.AttachmentName,
        CreatedAt = e.CreatedAt
    };
}

// ── Chi phí nhân sự (nhập theo tháng) ─────────────────────────────────────
public class PayrollService : IPayrollService
{
    private readonly CrmDbContext _db;
    public PayrollService(CrmDbContext db) { _db = db; }

    public async Task<PayrollPeriodDto> GetPeriodAsync(int year, int month)
    {
        var items = await _db.PayrollEntries.AsNoTracking()
            .Where(x => x.Year == year && x.Month == month)
            .OrderBy(x => x.EmployeeName)
            .Select(x => new PayrollEntryDto
            {
                Id = x.Id,
                Year = x.Year,
                Month = x.Month,
                UserId = x.UserId,
                EmployeeName = x.EmployeeName,
                Position = x.Position,
                Salary = x.Salary,
                Allowance = x.Allowance,
                Insurance = x.Insurance,
                OtherCost = x.OtherCost,
                TotalAmount = x.TotalAmount,
                Notes = x.Notes
            })
            .ToListAsync();

        return new PayrollPeriodDto
        {
            Year = year,
            Month = month,
            Items = items,
            TotalSalary = items.Sum(x => x.Salary),
            TotalAllowance = items.Sum(x => x.Allowance),
            TotalInsurance = items.Sum(x => x.Insurance),
            TotalOther = items.Sum(x => x.OtherCost),
            GrandTotal = items.Sum(x => x.TotalAmount)
        };
    }

    public async Task<PayrollEntryDto> CreateAsync(CreatePayrollEntryDto dto, Guid userId)
    {
        Validate(dto);

        if (dto.UserId.HasValue &&
            await _db.PayrollEntries.AnyAsync(x => x.Year == dto.Year && x.Month == dto.Month && x.UserId == dto.UserId))
            throw new InvalidOperationException("Nhân sự này đã có dòng lương trong kỳ.");

        var e = new PayrollEntry
        {
            Year = dto.Year,
            Month = dto.Month,
            UserId = dto.UserId,
            EmployeeName = dto.EmployeeName.Trim(),
            Position = dto.Position,
            Salary = dto.Salary,
            Allowance = dto.Allowance,
            Insurance = dto.Insurance,
            OtherCost = dto.OtherCost,
            Notes = dto.Notes,
            CreatedByUserId = userId
        };
        e.RecalculateTotal();

        _db.PayrollEntries.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<PayrollEntryDto> UpdateAsync(UpdatePayrollEntryDto dto)
    {
        var e = await _db.PayrollEntries.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy dòng lương.");
        Validate(dto);

        if (dto.UserId.HasValue && await _db.PayrollEntries.AnyAsync(x =>
                x.Id != dto.Id && x.Year == dto.Year && x.Month == dto.Month && x.UserId == dto.UserId))
            throw new InvalidOperationException("Nhân sự này đã có dòng lương trong kỳ.");

        e.Year = dto.Year;
        e.Month = dto.Month;
        e.UserId = dto.UserId;
        e.EmployeeName = dto.EmployeeName.Trim();
        e.Position = dto.Position;
        e.Salary = dto.Salary;
        e.Allowance = dto.Allowance;
        e.Insurance = dto.Insurance;
        e.OtherCost = dto.OtherCost;
        e.Notes = dto.Notes;
        e.UpdatedAt = DateTime.UtcNow;
        e.RecalculateTotal();

        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.PayrollEntries.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy dòng lương.");
        _db.PayrollEntries.Remove(e);
        await _db.SaveChangesAsync();
    }

    public async Task<int> CopyFromPreviousMonthAsync(int year, int month, Guid userId)
    {
        if (month is < 1 or > 12) throw new InvalidOperationException("Tháng không hợp lệ.");

        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;

        var source = await _db.PayrollEntries.AsNoTracking()
            .Where(x => x.Year == prevYear && x.Month == prevMonth)
            .ToListAsync();
        if (source.Count == 0)
            throw new InvalidOperationException($"Tháng {prevMonth:00}/{prevYear} chưa có dữ liệu lương để sao chép.");

        var existing = await _db.PayrollEntries
            .Where(x => x.Year == year && x.Month == month)
            .ToListAsync();
        var existingUserIds = existing.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).ToHashSet();
        var existingNames = existing.Where(x => !x.UserId.HasValue)
            .Select(x => x.EmployeeName.ToLowerInvariant()).ToHashSet();

        var added = 0;
        foreach (var s in source)
        {
            // Không nhân bản dòng đã có trong kỳ đích.
            if (s.UserId.HasValue ? existingUserIds.Contains(s.UserId.Value)
                                  : existingNames.Contains(s.EmployeeName.ToLowerInvariant()))
                continue;

            var e = new PayrollEntry
            {
                Year = year,
                Month = month,
                UserId = s.UserId,
                EmployeeName = s.EmployeeName,
                Position = s.Position,
                Salary = s.Salary,
                Allowance = s.Allowance,
                Insurance = s.Insurance,
                OtherCost = s.OtherCost,
                Notes = s.Notes,
                CreatedByUserId = userId
            };
            e.RecalculateTotal();
            _db.PayrollEntries.Add(e);
            added++;
        }

        if (added > 0) await _db.SaveChangesAsync();
        return added;
    }

    private static void Validate(CreatePayrollEntryDto dto)
    {
        if (dto.Month is < 1 or > 12) throw new InvalidOperationException("Tháng phải từ 1 đến 12.");
        if (dto.Year is < 2000 or > 2100) throw new InvalidOperationException("Năm không hợp lệ.");
        if (string.IsNullOrWhiteSpace(dto.EmployeeName)) throw new InvalidOperationException("Tên nhân sự không được để trống.");
        if (dto.Salary < 0 || dto.Allowance < 0 || dto.Insurance < 0 || dto.OtherCost < 0)
            throw new InvalidOperationException("Số tiền không được âm.");
    }

    private static PayrollEntryDto ToDto(PayrollEntry e) => new()
    {
        Id = e.Id,
        Year = e.Year,
        Month = e.Month,
        UserId = e.UserId,
        EmployeeName = e.EmployeeName,
        Position = e.Position,
        Salary = e.Salary,
        Allowance = e.Allowance,
        Insurance = e.Insurance,
        OtherCost = e.OtherCost,
        TotalAmount = e.TotalAmount,
        Notes = e.Notes
    };
}
