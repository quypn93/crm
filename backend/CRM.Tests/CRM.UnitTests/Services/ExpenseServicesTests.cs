using CRM.Application.DTOs.Finance;
using CRM.Infrastructure.Services.Finance;
using Microsoft.EntityFrameworkCore;

namespace CRM.UnitTests.Services;

public class ExpenseCategoryServiceTests : FinanceTestBase
{
    private ExpenseCategoryService Svc => new(Db);

    [Fact]
    public async Task Create_TrungTen_ThiBaoLoi()
    {
        await Svc.CreateAsync(new CreateExpenseCategoryDto { Name = "Tiền điện nước" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.CreateAsync(new CreateExpenseCategoryDto { Name = "tiền điện nước" }));
    }

    [Fact]
    public async Task Delete_DauMucMacDinh_ThiChan()
    {
        var systemCategory = AddCategory("Tiền thuê nhà", isSystem: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Svc.DeleteAsync(systemCategory.Id));
    }

    [Fact]
    public async Task Delete_DauMucDaPhatSinhChiPhi_ThiChan()
    {
        var category = AddCategory("Chi phí gửi xe");
        AddFixedExpense(category, new DateOnly(2026, 3, 1), 50_000);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Svc.DeleteAsync(category.Id));
    }

    [Fact]
    public async Task Delete_DauMucTuThem_ChuaDung_ThiXoaDuoc()
    {
        var created = await Svc.CreateAsync(new CreateExpenseCategoryDto { Name = "Chi phí bảo trì máy" });

        await Svc.DeleteAsync(created.Id);

        Assert.False(await Db.ExpenseCategories.AnyAsync(c => c.Id == created.Id));
    }

    [Fact]
    public async Task GetAll_ActiveOnly_BoQuaDauMucDaAn()
    {
        AddCategory("Đang dùng");
        var hidden = AddCategory("Đã ẩn");
        hidden.IsActive = false;
        Db.SaveChanges();

        var all = await Svc.GetAllAsync();
        var activeOnly = await Svc.GetAllAsync(activeOnly: true);

        Assert.Equal(2, all.Count());
        Assert.Single(activeOnly);
    }
}

public class FixedExpenseServiceTests : FinanceTestBase
{
    private FixedExpenseService Svc => new(Db);

    [Fact]
    public async Task Create_LuuSnapshotTenDauMuc()
    {
        var category = AddCategory("Tiền mạng internet");

        var created = await Svc.CreateAsync(new CreateFixedExpenseDto
        {
            ExpenseDate = new DateOnly(2026, 3, 8),
            ExpenseCategoryId = category.Id,
            Amount = 800_000
        }, AccountantId);

        Assert.Equal("Tiền mạng internet", created.CategoryName);
        Assert.Equal(new DateOnly(2026, 3, 8), created.ExpenseDate);
    }

    [Fact]
    public async Task GetList_LocTheoKhoangNgay_VaTongTheoDauMuc()
    {
        var dien = AddCategory("Tiền điện nước");
        var an = AddCategory("Chi phí ăn uống");
        AddFixedExpense(dien, new DateOnly(2026, 3, 1), 1_000_000);
        AddFixedExpense(dien, new DateOnly(2026, 3, 15), 500_000);
        AddFixedExpense(an, new DateOnly(2026, 3, 20), 2_000_000);
        AddFixedExpense(dien, new DateOnly(2026, 4, 1), 9_000_000);   // ngoài khoảng

        var result = await Svc.GetListAsync(new FixedExpenseFilterDto
        {
            DateFrom = new DateOnly(2026, 3, 1),
            DateTo = new DateOnly(2026, 3, 31)
        });

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3_500_000, result.GrandTotal);
        Assert.Equal(2, result.TotalsByCategory.Count);
        Assert.Equal("Chi phí ăn uống", result.TotalsByCategory[0].CategoryName);   // giảm dần theo tiền
        Assert.Equal(2_000_000, result.TotalsByCategory[0].Amount);
    }

    [Fact]
    public async Task Create_SoTienAm_ThiBaoLoi()
    {
        var category = AddCategory("Chi phí khác");

        await Assert.ThrowsAsync<InvalidOperationException>(() => Svc.CreateAsync(new CreateFixedExpenseDto
        {
            ExpenseDate = new DateOnly(2026, 3, 1),
            ExpenseCategoryId = category.Id,
            Amount = -1
        }, AccountantId));
    }
}

public class PayrollServiceTests : FinanceTestBase
{
    private PayrollService Svc => new(Db);

    [Fact]
    public async Task Create_TinhTong_VaCongDonKy()
    {
        await Svc.CreateAsync(new CreatePayrollEntryDto
        {
            Year = 2026, Month = 3, EmployeeName = "Nguyễn Văn A",
            Salary = 10_000_000, Allowance = 1_000_000, Insurance = 800_000, OtherCost = 200_000
        }, AccountantId);

        var period = await Svc.GetPeriodAsync(2026, 3);

        Assert.Single(period.Items);
        Assert.Equal(12_000_000, period.Items[0].TotalAmount);
        Assert.Equal(12_000_000, period.GrandTotal);
        Assert.Equal(10_000_000, period.TotalSalary);
    }

    [Fact]
    public async Task Create_CungNhanSu_CungKy_ThiBaoLoi()
    {
        var dto = new CreatePayrollEntryDto
        {
            Year = 2026, Month = 3, UserId = AccountantId, EmployeeName = "Ke Toan", Salary = 5_000_000
        };
        await Svc.CreateAsync(dto, AccountantId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Svc.CreateAsync(dto, AccountantId));
    }

    [Fact]
    public async Task Create_ThangKhongHopLe_ThiBaoLoi()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Svc.CreateAsync(new CreatePayrollEntryDto
        {
            Year = 2026, Month = 13, EmployeeName = "X", Salary = 1
        }, AccountantId));
    }

    [Fact]
    public async Task CopyFromPrevious_SaoChepDuDong()
    {
        AddPayroll(2026, 2, "Nhân viên 1", salary: 10_000_000);
        AddPayroll(2026, 2, "Nhân viên 2", salary: 8_000_000, allowance: 500_000);

        var added = await Svc.CopyFromPreviousMonthAsync(2026, 3, AccountantId);
        var march = await Svc.GetPeriodAsync(2026, 3);

        Assert.Equal(2, added);
        Assert.Equal(18_500_000, march.GrandTotal);
    }

    [Fact]
    public async Task CopyFromPrevious_KhongNhanBanDongDaCo()
    {
        AddPayroll(2026, 2, "Nhân viên 1", salary: 10_000_000);
        AddPayroll(2026, 3, "Nhân viên 1", salary: 11_000_000);   // đã nhập tay cho T3

        var added = await Svc.CopyFromPreviousMonthAsync(2026, 3, AccountantId);
        var march = await Svc.GetPeriodAsync(2026, 3);

        Assert.Equal(0, added);
        Assert.Single(march.Items);
        Assert.Equal(11_000_000, march.Items[0].Salary);
    }

    [Fact]
    public async Task CopyFromPrevious_ThangTruocTrong_ThiBaoLoi()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.CopyFromPreviousMonthAsync(2026, 3, AccountantId));
    }

    [Fact]
    public async Task CopyFromPrevious_ThangMot_LayTuThang12NamTruoc()
    {
        AddPayroll(2025, 12, "Nhân viên 1", salary: 7_000_000);

        var added = await Svc.CopyFromPreviousMonthAsync(2026, 1, AccountantId);

        Assert.Equal(1, added);
        Assert.Equal(7_000_000, (await Svc.GetPeriodAsync(2026, 1)).GrandTotal);
    }
}
