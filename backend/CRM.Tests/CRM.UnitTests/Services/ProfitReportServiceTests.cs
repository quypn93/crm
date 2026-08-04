using CRM.Application.DTOs.Finance;
using CRM.Core.Enums;
using CRM.Infrastructure.Services.Finance;

namespace CRM.UnitTests.Services;

public class ProfitReportServiceTests : FinanceTestBase
{
    private ProfitReportService Svc => new(Db);

    [Fact]
    public async Task MonthlyProfit_LaiRong_TruDuNhanSu_VaCoDinh()
    {
        var order = AddOrder("ORD-001", 100_000_000, VnNoonUtc(2026, 3, 15));
        AddCost(order.Id, cost: 55_000_000, ship: 3_000_000, outbound: 2_000_000);

        AddPayroll(2026, 3, "Nhân viên 1", salary: 10_000_000, insurance: 1_000_000);
        AddPayroll(2026, 3, "Nhân viên 2", salary: 8_000_000);

        var dien = AddCategory("Tiền điện nước");
        AddFixedExpense(dien, new DateOnly(2026, 3, 5), 2_000_000);
        AddFixedExpense(dien, new DateOnly(2026, 3, 20), 1_500_000);

        var result = await Svc.GetMonthlyProfitAsync(2026, "confirmed");
        var march = result.Months.First(m => m.Month == 3);

        Assert.Equal(100_000_000, march.Revenue);
        Assert.Equal(60_000_000, march.Cogs);
        Assert.Equal(40_000_000, march.GrossProfit);
        Assert.Equal(19_000_000, march.PayrollCost);
        Assert.Equal(3_500_000, march.FixedCost);
        Assert.Equal(17_500_000, march.NetProfit);
        Assert.Equal(17.5m, march.NetMargin);
    }

    [Fact]
    public async Task MonthlyProfit_ThangLo_TraVeSoAm()
    {
        var order = AddOrder("ORD-LO", 10_000_000, VnNoonUtc(2026, 4, 10));
        AddCost(order.Id, cost: 8_000_000);
        AddPayroll(2026, 4, "Nhân viên 1", salary: 15_000_000);

        var result = await Svc.GetMonthlyProfitAsync(2026, "confirmed");
        var april = result.Months.First(m => m.Month == 4);

        Assert.Equal(2_000_000, april.GrossProfit);
        Assert.Equal(-13_000_000, april.NetProfit);
        Assert.True(april.NetMargin < 0);
    }

    [Fact]
    public async Task MonthlyProfit_TraDu12Thang_VaDongTongCaNam()
    {
        AddCost(AddOrder("ORD-T1", 5_000_000, VnNoonUtc(2026, 1, 10)).Id, 2_000_000);
        AddCost(AddOrder("ORD-T6", 7_000_000, VnNoonUtc(2026, 6, 10)).Id, 3_000_000);

        var result = await Svc.GetMonthlyProfitAsync(2026, "confirmed");

        Assert.Equal(12, result.Months.Count);
        Assert.Equal(12_000_000, result.Total.Revenue);
        Assert.Equal(5_000_000, result.Total.Cogs);
        Assert.Equal(7_000_000, result.Total.NetProfit);
        Assert.Equal(0, result.Months.First(m => m.Month == 2).Revenue);
    }

    [Fact]
    public async Task MonthlyProfit_KhongTinhDonHuy()
    {
        AddCost(AddOrder("ORD-OK", 10_000_000, VnNoonUtc(2026, 5, 10)).Id, 4_000_000);
        var cancelled = AddOrder("ORD-HUY", 90_000_000, VnNoonUtc(2026, 5, 12), OrderStatus.Cancelled);
        AddCost(cancelled.Id, 50_000_000);

        var result = await Svc.GetMonthlyProfitAsync(2026, "confirmed");
        var may = result.Months.First(m => m.Month == 5);

        Assert.Equal(10_000_000, may.Revenue);
        Assert.Equal(4_000_000, may.Cogs);
        Assert.Equal(1, may.OrderCount);
    }

    [Fact]
    public async Task OrderProfit_DonChuaNhapCost_KhongCongVaoLai()
    {
        var withCost = AddOrder("ORD-CO", 20_000_000, VnNoonUtc(2026, 7, 10));
        AddCost(withCost.Id, 12_000_000);
        AddOrder("ORD-CHUA", 80_000_000, VnNoonUtc(2026, 7, 11));

        var result = await Svc.GetOrderProfitAsync(new ProfitFilterDto
        {
            DateFrom = VnNoonUtc(2026, 7, 1),
            DateTo = VnNoonUtc(2026, 7, 31)
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(100_000_000, result.TotalRevenue);
        Assert.Equal(12_000_000, result.TotalCost);
        Assert.Equal(8_000_000, result.TotalProfit);            // chỉ đơn đã có cost
        Assert.Equal(80_000_000, result.RevenueWithoutCost);
        Assert.Equal(1, result.OrdersWithoutCost);
        Assert.Equal(40m, result.AverageMargin);                // 8tr / 20tr
    }

    [Fact]
    public async Task OrderProfit_DoanhThuBang0_KhongChiaChoKhong()
    {
        var order = AddOrder("ORD-0D", 0, VnNoonUtc(2026, 8, 10));
        AddCost(order.Id, 500_000);

        var result = await Svc.GetOrderProfitAsync(new ProfitFilterDto());
        var item = result.Items.First(i => i.OrderNumber == "ORD-0D");

        Assert.Equal(-500_000, item.Profit);
        Assert.Equal(0, item.ProfitMargin);
    }

    [Fact]
    public async Task MonthDetail_BocTachChiPhiTheoDauMuc()
    {
        var order = AddOrder("ORD-CT", 30_000_000, VnNoonUtc(2026, 9, 10));
        AddCost(order.Id, 18_000_000);
        AddPayroll(2026, 9, "Nhân viên 1", salary: 9_000_000);

        var dien = AddCategory("Tiền điện nước");
        var nha = AddCategory("Tiền thuê nhà");
        AddFixedExpense(dien, new DateOnly(2026, 9, 3), 1_000_000);
        AddFixedExpense(nha, new DateOnly(2026, 9, 1), 5_000_000);
        AddFixedExpense(dien, new DateOnly(2026, 10, 1), 999_000);   // tháng khác — không tính

        var detail = await Svc.GetMonthDetailAsync(2026, 9, "confirmed");

        Assert.Equal(6_000_000, detail.Summary.FixedCost);
        Assert.Equal(2, detail.FixedByCategory.Count);
        Assert.Equal("Tiền thuê nhà", detail.FixedByCategory[0].CategoryName);   // sắp theo số tiền giảm dần
        Assert.Single(detail.PayrollEntries);
        Assert.Single(detail.Orders);
        Assert.Equal(-3_000_000, detail.Summary.NetProfit);   // 30 - 18 - 9 - 6
    }

    [Fact]
    public async Task RevenueBasis_TheoNgayHoanThanh_LoaiDonChuaHoanThanh()
    {
        var done = AddOrder("ORD-XONG", 10_000_000, VnNoonUtc(2026, 2, 5));
        done.CompletionDate = VnNoonUtc(2026, 3, 20);       // xác nhận T2, xong T3
        AddOrder("ORD-DANGLAM", 20_000_000, VnNoonUtc(2026, 2, 6));
        Db.SaveChanges();

        var byConfirmed = await Svc.GetMonthlyProfitAsync(2026, "confirmed");
        Assert.Equal(30_000_000, byConfirmed.Months.First(m => m.Month == 2).Revenue);

        var byCompleted = await Svc.GetMonthlyProfitAsync(2026, "completed");
        Assert.Equal(0, byCompleted.Months.First(m => m.Month == 2).Revenue);
        Assert.Equal(10_000_000, byCompleted.Months.First(m => m.Month == 3).Revenue);
    }
}
