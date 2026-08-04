using CRM.Application.DTOs.Finance;
using CRM.Core.Enums;
using CRM.Infrastructure.Services.Finance;
using Microsoft.EntityFrameworkCore;

namespace CRM.UnitTests.Services;

public class OrderCostServiceTests : FinanceTestBase
{
    private OrderCostService Svc => new(Db);

    [Fact]
    public async Task Upsert_TinhTongChiPhi_Va_LaiLo()
    {
        var order = AddOrder("ORD-001", 10_000_000, VnNoonUtc(2026, 3, 10));

        var result = await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto
        {
            CostAmount = 6_000_000,
            ShippingCost = 200_000,
            OutboundShippingCost = 100_000,
            OtherCost = 50_000
        }, AccountantId, isAdmin: false);

        Assert.Equal(6_350_000, result.TotalCost);
        Assert.Equal(3_650_000, result.Profit);
        Assert.Equal(36.5m, result.ProfitMargin);
        Assert.True(result.HasCost);
    }

    [Fact]
    public async Task Upsert_LanHai_KhongTaoBanGhiTrung()
    {
        var order = AddOrder("ORD-002", 5_000_000, VnNoonUtc(2026, 3, 10));

        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { CostAmount = 1_000_000 }, AccountantId, false);
        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { CostAmount = 2_000_000 }, AccountantId, false);

        var costs = await Db.OrderCosts.Where(c => c.OrderId == order.Id).ToListAsync();
        Assert.Single(costs);
        Assert.Equal(2_000_000, costs[0].CostAmount);
    }

    [Fact]
    public async Task Upsert_SoTienAm_ThiBaoLoi()
    {
        var order = AddOrder("ORD-003", 1_000_000, VnNoonUtc(2026, 3, 10));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { CostAmount = -1 }, AccountantId, false));
    }

    [Fact]
    public async Task Upsert_DonDaChotSo_KeToanKhongSuaDuoc_AdminSuaDuoc()
    {
        var order = AddOrder("ORD-004", 1_000_000, VnNoonUtc(2026, 3, 10));
        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { CostAmount = 500_000, IsFinalized = true }, AccountantId, false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { CostAmount = 999 }, AccountantId, isAdmin: false));

        var byAdmin = await Svc.UpsertAsync(order.Id,
            new UpsertOrderCostDto { CostAmount = 999, IsFinalized = false }, AccountantId, isAdmin: true);

        Assert.Equal(999, byAdmin.CostAmount);
        Assert.False(byAdmin.IsFinalized);
    }

    [Fact]
    public async Task GetList_ChiLayDonTuDangSanXuat_LoaiDonHuy()
    {
        var inProduction = AddOrder("ORD-SX", 1_000_000, VnNoonUtc(2026, 3, 10), OrderStatus.InProduction);
        AddOrder("ORD-NHAP", 2_000_000, VnNoonUtc(2026, 3, 10), OrderStatus.Draft);
        AddOrder("ORD-XACNHAN", 3_000_000, VnNoonUtc(2026, 3, 10), OrderStatus.Confirmed);
        AddOrder("ORD-HUY", 4_000_000, VnNoonUtc(2026, 3, 10), OrderStatus.Cancelled);
        var completed = AddOrder("ORD-XONG", 5_000_000, VnNoonUtc(2026, 3, 10), OrderStatus.Completed);

        var result = await Svc.GetListAsync(new OrderCostFilterDto());

        var numbers = result.Items.Select(i => i.OrderNumber).ToList();
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(inProduction.OrderNumber, numbers);
        Assert.Contains(completed.OrderNumber, numbers);
        Assert.DoesNotContain("ORD-HUY", numbers);
        Assert.DoesNotContain("ORD-NHAP", numbers);
    }

    [Fact]
    public async Task GetList_LocDonChuaNhapCost()
    {
        var withCost = AddOrder("ORD-CO", 1_000_000, VnNoonUtc(2026, 3, 10));
        AddCost(withCost.Id, 400_000);
        AddOrder("ORD-CHUA", 2_000_000, VnNoonUtc(2026, 3, 10));

        var missing = await Svc.GetListAsync(new OrderCostFilterDto { HasCost = false });

        Assert.Single(missing.Items);
        Assert.Equal("ORD-CHUA", missing.Items[0].OrderNumber);
        Assert.False(missing.Items[0].HasCost);
    }

    [Fact]
    public async Task GetList_TongLai_ChiTinhDonDaCoCost_TranhLaiAo()
    {
        var withCost = AddOrder("ORD-CO", 10_000_000, VnNoonUtc(2026, 3, 10));
        AddCost(withCost.Id, 6_000_000);
        AddOrder("ORD-CHUA", 50_000_000, VnNoonUtc(2026, 3, 10));   // chưa nhập cost

        var result = await Svc.GetListAsync(new OrderCostFilterDto());

        Assert.Equal(60_000_000, result.Summary.TotalRevenue);
        Assert.Equal(6_000_000, result.Summary.TotalCost);
        // Nếu tính cả đơn chưa nhập cost thì lãi sẽ là 54tr — con số ảo.
        Assert.Equal(4_000_000, result.Summary.TotalProfit);
        Assert.Equal(1, result.Summary.OrdersWithoutCost);
    }

    [Fact]
    public async Task BulkUpsert_LuuNhieuDon_BoQuaDonDaChotSo()
    {
        var a = AddOrder("ORD-A", 1_000_000, VnNoonUtc(2026, 3, 10));
        var b = AddOrder("ORD-B", 2_000_000, VnNoonUtc(2026, 3, 10));
        await Svc.UpsertAsync(b.Id, new UpsertOrderCostDto { CostAmount = 100, IsFinalized = true }, AccountantId, false);

        var saved = await Svc.BulkUpsertAsync(new BulkUpsertOrderCostDto
        {
            Items = new List<BulkOrderCostItemDto>
            {
                new() { OrderId = a.Id, CostAmount = 500_000 },
                new() { OrderId = b.Id, CostAmount = 900_000 }
            }
        }, AccountantId, isAdmin: false);

        Assert.Equal(1, saved);
        Assert.Equal(500_000, (await Db.OrderCosts.FirstAsync(c => c.OrderId == a.Id)).CostAmount);
        Assert.Equal(100, (await Db.OrderCosts.FirstAsync(c => c.OrderId == b.Id)).CostAmount);
    }

    [Fact]
    public async Task Import_Csv_ApCostTheoMaDon_DongLoiKhongChanDongDung()
    {
        var a = AddOrder("ORD-100", 10_000_000, VnNoonUtc(2026, 3, 10));
        AddOrder("ORD-200", 20_000_000, VnNoonUtc(2026, 3, 10));

        // Dòng 3 mã không tồn tại, dòng 4 số tiền không hợp lệ.
        var csv = string.Join("\n",
            "Mã đơn hàng,Giá cost,Chi phí ship hàng,Chi phí gửi hàng đi,Chi phí khác,Ghi chú",
            "ORD-100,1.200.000,35000,20000,0,ghi chú A",
            "ORD-999,500000,0,0,0,",
            "ORD-200,abc,0,0,0,");

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        var result = await Svc.ImportAsync(stream, "cost.csv", AccountantId, isAdmin: false);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.Errors.Count);

        var cost = await Db.OrderCosts.FirstAsync(c => c.OrderId == a.Id);
        Assert.Equal(1_200_000, cost.CostAmount);      // "1.200.000" kiểu VN đọc đúng
        Assert.Equal(1_255_000, cost.TotalCost);
    }

    [Fact]
    public async Task Import_OTrong_GiuNguyenGiaTriCu()
    {
        var order = AddOrder("ORD-300", 10_000_000, VnNoonUtc(2026, 3, 10));
        AddCost(order.Id, cost: 5_000_000, ship: 111_000);

        var csv = "Mã đơn hàng,Giá cost,Chi phí ship hàng,Chi phí gửi hàng đi,Chi phí khác,Ghi chú\n"
                + "ORD-300,7000000,,,,";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        await Svc.ImportAsync(stream, "cost.csv", AccountantId, false);

        var updated = await Db.OrderCosts.FirstAsync(c => c.OrderId == order.Id);
        Assert.Equal(7_000_000, updated.CostAmount);
        Assert.Equal(111_000, updated.ShippingCost);    // không bị ghi đè về 0
        Assert.Equal(7_111_000, updated.TotalCost);
    }
}
