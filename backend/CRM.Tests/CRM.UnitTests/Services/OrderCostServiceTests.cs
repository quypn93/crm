using CRM.Application.DTOs.Finance;
using CRM.Core.Enums;
using CRM.Infrastructure.Services.Finance;
using Microsoft.EntityFrameworkCore;

namespace CRM.UnitTests.Services;

public class OrderCostServiceTests : FinanceTestBase
{
    private OrderCostService Svc => new(Db);

    [Fact]
    public async Task Upsert_DonGia_NhanTongSoLuongMoiDongSize()
    {
        // 4 dòng size của cùng 1 sản phẩm → tổng SL 100.
        var order = AddOrder("ORD-001", 10_000_000, VnNoonUtc(2026, 3, 10),
            quantities: new[] { 23, 34, 28, 15 });

        var result = await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto
        {
            UnitCost = 60_000,
            ShippingCost = 200_000,
            OutboundShippingCost = 100_000,
            OtherCost = 50_000
        }, AccountantId, isAdmin: false);

        Assert.Equal(100, result.TotalQuantity);
        Assert.Equal(6_000_000, result.CostAmount);      // 60.000 × 100, KHÔNG phải × 23 (dòng đầu)
        Assert.Equal(6_350_000, result.TotalCost);
        Assert.Equal(3_650_000, result.Profit);
        Assert.Equal(36.5m, result.ProfitMargin);
        Assert.True(result.HasCost);
    }

    [Fact]
    public async Task Upsert_QuaTang_DungSoLuongRieng_KhongPhaiSoLuongAo()
    {
        var order = AddOrder("ORD-QUA", 50_000_000, VnNoonUtc(2026, 3, 10),
            quantities: new[] { 100, 100 });   // 200 áo

        var result = await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto
        {
            UnitCost = 60_000,
            GiftUnitCost = 150_000,
            GiftQuantity = 1              // tặng 1 lá cờ cho cả đơn
        }, AccountantId, isAdmin: false);

        Assert.Equal(200, result.TotalQuantity);
        Assert.Equal(12_000_000, result.CostAmount);
        Assert.Equal(150_000, result.GiftAmount);        // nếu nhân 200 sẽ ra 30 triệu — sai
        Assert.Equal(12_150_000, result.TotalCost);
    }

    [Fact]
    public async Task Upsert_SoLuongQuaAm_ThiBaoLoi()
    {
        var order = AddOrder("ORD-QUA-AM", 1_000_000, VnNoonUtc(2026, 3, 10));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { GiftQuantity = -1 }, AccountantId, false));
    }

    [Fact]
    public async Task Upsert_LuuMaGiaoHang_VaSoTienThanhToan()
    {
        var order = AddOrder("ORD-GH", 5_000_000, VnNoonUtc(2026, 3, 10));

        var result = await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto
        {
            UnitCost = 1_000,
            ShippingCode = " VD123456789 ",
            SettlementAmount = 2_500_000
        }, AccountantId, isAdmin: false);

        Assert.Equal("VD123456789", result.ShippingCode);
        Assert.False(result.ShippingCodeFromCarrier);
        Assert.Equal(2_500_000, result.SettlementAmount);

        // Không được đụng vào Order.PaidAmount — nó chi phối tiền hãng vận chuyển thu hộ.
        var reloaded = await Db.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        Assert.Equal(0, reloaded.PaidAmount);
    }

    [Fact]
    public async Task Upsert_MaHangVanChuyen_ThiKhongCho_GhiDeBangMaNhapTay()
    {
        var order = AddOrder("ORD-GHTK", 5_000_000, VnNoonUtc(2026, 3, 10));
        order.GhtkLabel = "GHTK-ABC";
        Db.SaveChanges();

        var result = await Svc.UpsertAsync(order.Id,
            new UpsertOrderCostDto { ShippingCode = "NHAP-TAY" }, AccountantId, isAdmin: false);

        Assert.Equal("GHTK-ABC", result.ShippingCode);
        Assert.True(result.ShippingCodeFromCarrier);
    }

    [Fact]
    public async Task Upsert_LanHai_KhongTaoBanGhiTrung()
    {
        var order = AddOrder("ORD-002", 5_000_000, VnNoonUtc(2026, 3, 10));

        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { UnitCost = 1_000_000 }, AccountantId, false);
        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { UnitCost = 2_000_000 }, AccountantId, false);

        var costs = await Db.OrderCosts.Where(c => c.OrderId == order.Id).ToListAsync();
        Assert.Single(costs);
        Assert.Equal(2_000_000, costs[0].UnitCost);
    }

    [Fact]
    public async Task Upsert_SoTienAm_ThiBaoLoi()
    {
        var order = AddOrder("ORD-003", 1_000_000, VnNoonUtc(2026, 3, 10));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { UnitCost = -1 }, AccountantId, false));
    }

    [Fact]
    public async Task Upsert_DonDaChotSo_KeToanKhongSuaDuoc_AdminSuaDuoc()
    {
        var order = AddOrder("ORD-004", 1_000_000, VnNoonUtc(2026, 3, 10));
        await Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { UnitCost = 500_000, IsFinalized = true }, AccountantId, false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Svc.UpsertAsync(order.Id, new UpsertOrderCostDto { UnitCost = 999 }, AccountantId, isAdmin: false));

        var byAdmin = await Svc.UpsertAsync(order.Id,
            new UpsertOrderCostDto { UnitCost = 999, IsFinalized = false }, AccountantId, isAdmin: true);

        Assert.Equal(999, byAdmin.UnitCost);
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
        await Svc.UpsertAsync(b.Id, new UpsertOrderCostDto { UnitCost = 100, IsFinalized = true }, AccountantId, false);

        var saved = await Svc.BulkUpsertAsync(new BulkUpsertOrderCostDto
        {
            Items = new List<BulkOrderCostItemDto>
            {
                new() { OrderId = a.Id, UnitCost = 500_000 },
                new() { OrderId = b.Id, UnitCost = 900_000 }
            }
        }, AccountantId, isAdmin: false);

        Assert.Equal(1, saved);
        Assert.Equal(500_000, (await Db.OrderCosts.FirstAsync(c => c.OrderId == a.Id)).UnitCost);
        Assert.Equal(100, (await Db.OrderCosts.FirstAsync(c => c.OrderId == b.Id)).UnitCost);
    }

    private const string ImportHeader =
        "Mã đơn hàng,Đơn giá cost,Đơn giá quà tặng,SL quà tặng,Chi phí ship hàng,Chi phí gửi hàng đi,Chi phí khác,Mã giao hàng,Số tiền thanh toán,Ghi chú";

    private static MemoryStream CsvOf(params string[] dataRows)
    {
        var text = string.Join(Environment.NewLine, new[] { ImportHeader }.Concat(dataRows));
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
    }

    [Fact]
    public async Task Import_Csv_ApCostTheoMaDon_DongLoiKhongChanDongDung()
    {
        var a = AddOrder("ORD-100", 10_000_000, VnNoonUtc(2026, 3, 10), quantities: new[] { 10, 15 }); // 25 SP
        AddOrder("ORD-200", 20_000_000, VnNoonUtc(2026, 3, 10));

        // Dòng 3 mã đơn không tồn tại, dòng 4 đơn giá không phải số.
        using var stream = CsvOf(
            "ORD-100,48.000,15000,2,35000,20000,0,VD999,5.000.000,ghi chu A",
            "ORD-999,500000,0,0,0,0,0,,0,",
            "ORD-200,abc,0,0,0,0,0,,0,");

        var result = await Svc.ImportAsync(stream, "cost.csv", AccountantId, isAdmin: false);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.Errors.Count);

        var cost = await Db.OrderCosts.FirstAsync(c => c.OrderId == a.Id);
        Assert.Equal(48_000, cost.UnitCost);            // "48.000" kiểu VN đọc đúng
        Assert.Equal(25, cost.TotalQuantity);
        Assert.Equal(1_200_000, cost.CostAmount);       // 48.000 × 25
        Assert.Equal(30_000, cost.GiftAmount);          // 15.000 × 2
        Assert.Equal("VD999", cost.ShippingCode);
        Assert.Equal(5_000_000, cost.SettlementAmount);
        Assert.Equal(1_285_000, cost.TotalCost);        // 1.200.000 + 30.000 + 35.000 + 20.000
    }

    [Fact]
    public async Task Import_OTrong_GiuNguyenGiaTriCu()
    {
        var order = AddOrder("ORD-300", 10_000_000, VnNoonUtc(2026, 3, 10));
        AddCost(order.Id, cost: 5_000_000, ship: 111_000);

        using var stream = CsvOf("ORD-300,7000000,,,,,,,,");
        await Svc.ImportAsync(stream, "cost.csv", AccountantId, false);

        var updated = await Db.OrderCosts.FirstAsync(c => c.OrderId == order.Id);
        Assert.Equal(7_000_000, updated.UnitCost);
        Assert.Equal(111_000, updated.ShippingCost);    // không bị ghi đè về 0
        Assert.Equal(7_111_000, updated.TotalCost);
    }

    [Fact]
    public async Task Import_SoLuongQuaLeThapPhan_ThiBaoLoi()
    {
        AddOrder("ORD-400", 1_000_000, VnNoonUtc(2026, 3, 10));

        using var stream = CsvOf("ORD-400,1000,500,2.5,0,0,0,,0,");
        var result = await Svc.ImportAsync(stream, "cost.csv", AccountantId, false);

        Assert.Equal(0, result.SuccessCount);
        Assert.Contains("nguyên", result.Errors[0].Error);
    }

    [Theory]
    [InlineData("48.000", 48000)]        // 1 dấu chấm, 3 số → nghìn kiểu VN
    [InlineData("1.200.000", 1200000)]
    [InlineData("1,200,000", 1200000)]
    [InlineData("1200000", 1200000)]
    [InlineData("1200000.50", 1200000.5)]
    [InlineData("48,000", 48000)]
    [InlineData("72000 đ", 72000)]
    public async Task Import_DocDungCacDinhDangSoTien(string raw, decimal expected)
    {
        var order = AddOrder("ORD-FMT", 1_000_000, VnNoonUtc(2026, 3, 10));

        // Bọc nháy kép vì giá trị có thể chứa dấu phẩy — chính là dấu phân cách CSV.
        using var stream = CsvOf($"ORD-FMT,\"{raw}\",0,0,0,0,0,,0,");
        var result = await Svc.ImportAsync(stream, "cost.csv", AccountantId, false);

        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(expected, (await Db.OrderCosts.FirstAsync(c => c.OrderId == order.Id)).UnitCost);
    }
}
