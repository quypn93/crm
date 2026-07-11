using System.Globalization;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.ViettelPost;

public class ViettelPostShipmentService : IViettelPostShipmentService
{
    private readonly IViettelPostClient _client;
    private readonly ViettelPostOptions _opts;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ViettelPostShipmentService> _log;

    public ViettelPostShipmentService(IViettelPostClient client, IOptions<ViettelPostOptions> opts,
        IUnitOfWork uow, ILogger<ViettelPostShipmentService> log)
    {
        _client = client;
        _opts = opts.Value;
        _uow = uow;
        _log = log;
    }

    public bool IsConfigured => _client.IsConfigured;

    public async Task<ViettelPostFeeDto> EstimateFeeAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        ValidateAddress(order);

        var pick = await ResolvePickAsync(order, ct);
        var (provinceId, districtId, _) = await ResolveReceiverAsync(order, ct);
        var query = new VtpFeeQuery
        {
            SenderProvince = pick.ProvinceId,
            SenderDistrict = pick.DistrictId,
            ReceiverProvince = provinceId,
            ReceiverDistrict = districtId,
            WeightGram = EstimateWeightGram(order),
            Value = order.TotalAmount,
            MoneyCollection = _opts.Defaults.UseCod ? Math.Max(0, order.TotalAmount - order.PaidAmount) : 0
        };

        var fee = await _client.GetFeeAsync(query, ct);
        var totalFee = fee.MoneyTotalFee ?? fee.MoneyFee ?? fee.MoneyTotal ?? 0;

        order.ViettelPostFee = totalFee;
        order.ViettelPostSyncedAt = DateTime.UtcNow;
        order.ViettelPostLastError = null;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return new ViettelPostFeeDto
        {
            Fee = totalFee,
            InsuranceFee = 0,
            DeliveryType = fee.OrderService
        };
    }

    public async Task<ViettelPostShipmentDto> CreateShipmentAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.DeliveryMethod != DeliveryMethod.ViettelPost)
            throw new InvalidOperationException("Đơn hàng không chọn hình thức Viettel Post.");
        if (!string.IsNullOrWhiteSpace(order.ViettelPostLabel))
            throw new InvalidOperationException($"Đơn đã có vận đơn Viettel Post {order.ViettelPostLabel}.");

        ValidateAddress(order);

        var pick = await ResolvePickAsync(order, ct);
        var (provinceId, districtId, wardId) = await ResolveReceiverAsync(order, ct);
        var cod = _opts.Defaults.UseCod ? Math.Max(0, order.TotalAmount - order.PaidAmount) : 0;

        var req = new VtpCreateOrderRequest
        {
            OrderNumber = order.OrderNumber,
            GroupAddressId = _opts.Pick.GroupAddressId,
            CusId = _opts.Pick.CusId,
            SenderFullname = pick.Name,
            SenderPhone = pick.Tel,
            SenderAddress = pick.Address,
            SenderProvince = pick.ProvinceId,
            SenderDistrict = pick.DistrictId,
            SenderWard = pick.WardId,
            ReceiverFullname = order.ShippingContactName ?? order.CustomerName ?? string.Empty,
            ReceiverPhone = order.ShippingPhone ?? string.Empty,
            ReceiverAddress = order.ShippingAddress ?? string.Empty,
            ReceiverProvince = provinceId,
            ReceiverDistrict = districtId,
            ReceiverWard = wardId,
            ProductName = BuildProductName(order),
            ProductDescription = order.OrderNumber,
            ProductQuantity = Math.Max(1, order.Items?.Sum(i => i.Quantity) ?? 1),
            ProductPrice = order.TotalAmount,
            ProductWeight = EstimateWeightGram(order),
            ProductType = _opts.Defaults.ProductType,
            OrderPayment = _opts.Defaults.OrderPayment,
            OrderService = _opts.Defaults.OrderService,
            OrderServiceAdd = _opts.Defaults.OrderServiceAdd,
            OrderNote = order.ShippingNotes,
            MoneyCollection = cod,
            NationalType = _opts.Defaults.NationalType,
            ListItem = BuildItems(order)
        };

        try
        {
            var resp = await _client.CreateOrderAsync(req, ct);

            order.ViettelPostLabel = resp.OrderNumber ?? order.OrderNumber;
            order.ViettelPostTrackingUrl = BuildTrackingUrl(order.ViettelPostLabel);
            order.ViettelPostFee = resp.MoneyTotalFee ?? resp.MoneyFee ?? resp.MoneyTotal;
            order.ViettelPostStatus = "created";
            order.ViettelPostStatusCode = null;
            order.ViettelPostSyncedAt = DateTime.UtcNow;
            order.ViettelPostLastError = null;

            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();

            return ToDto(order);
        }
        catch (ViettelPostException ex)
        {
            order.ViettelPostLastError = ex.Message;
            order.ViettelPostSyncedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
            throw;
        }
    }

    public async Task<bool> CancelShipmentAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (string.IsNullOrWhiteSpace(order.ViettelPostLabel))
            throw new InvalidOperationException("Đơn chưa có vận đơn Viettel Post.");

        var ok = await _client.CancelOrderAsync(order.ViettelPostLabel, "Huỷ từ CRM", ct);
        if (ok)
        {
            order.ViettelPostStatus = "cancelled";
            order.ViettelPostStatusCode = 107;
            order.ViettelPostSyncedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
        }
        return ok;
    }

    public async Task<ViettelPostShipmentDto?> SyncStatusAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (string.IsNullOrWhiteSpace(order.ViettelPostLabel)) return null;

        var status = await _client.GetStatusAsync(order.ViettelPostLabel, ct);
        if (status == null) return null;

        order.ViettelPostStatusCode = status.OrderStatus;
        order.ViettelPostStatus = status.StatusName ?? MapStatusCode(status.OrderStatus);
        order.ViettelPostSyncedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task HandleWebhookAsync(string orderNumber, int statusCode, string? statusText, decimal? fee, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.ViettelPostLabel == orderNumber);
        if (order == null)
        {
            _log.LogWarning("Viettel Post webhook nhận mã không khớp order: {OrderNumber}", orderNumber);
            return;
        }

        order.ViettelPostStatusCode = statusCode;
        order.ViettelPostStatus = statusText ?? MapStatusCode(statusCode);
        if (fee.HasValue) order.ViettelPostFee = fee.Value;
        order.ViettelPostSyncedAt = DateTime.UtcNow;

        // Đồng bộ trạng thái đơn của mình theo Viettel Post.
        if (IsDeliveredStatus(statusCode))
        {
            if (order.Status == OrderStatus.Shipping)
            {
                order.Status = OrderStatus.Delivered;
                order.ActualDeliveryDate = DateTime.UtcNow;
            }
        }
        else if (IsInTransitStatus(statusCode))
        {
            if (order.Status == OrderStatus.ReadyToShip)
                order.Status = OrderStatus.Shipping;
        }

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private sealed record ResolvedPick(string Name, string Tel, string Address, int ProvinceId, int DistrictId, int WardId);

    // Kho gửi: ưu tiên địa chỉ chọn trên đơn → địa chỉ mặc định → cấu hình appsettings.
    private async Task<ResolvedPick> ResolvePickAsync(Order order, CancellationToken ct)
    {
        SenderAddress? sa = null;
        if (order.SenderAddressId.HasValue)
            sa = await _uow.SenderAddresses.GetByIdAsync(order.SenderAddressId.Value);
        sa ??= (await _uow.SenderAddresses.FindAsync(x => x.IsDefault && x.IsActive)).FirstOrDefault();

        if (sa != null)
            return new ResolvedPick(sa.Name, sa.Phone, sa.Address, sa.ProvinceId, sa.DistrictId, sa.WardId);

        var p = _opts.Pick;
        return new ResolvedPick(p.Name, p.Tel, p.Address, p.ProvinceId, p.DistrictId, p.WardId);
    }

    private void ValidateAddress(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.ShippingProvinceName))
            throw new InvalidOperationException("Đơn hàng thiếu tỉnh/thành giao nhận.");
        if (string.IsNullOrWhiteSpace(order.ShippingAddress))
            throw new InvalidOperationException("Đơn hàng thiếu địa chỉ giao nhận.");
        if (string.IsNullOrWhiteSpace(order.ShippingPhone))
            throw new InvalidOperationException("Đơn hàng thiếu số điện thoại người nhận.");
        if (!_client.IsConfigured)
            throw new InvalidOperationException("Viettel Post chưa cấu hình — không thể tạo vận đơn.");
    }

    // CRM lưu Tỉnh + Phường/Xã (bỏ cấp Huyện sau 2025) nhưng Viettel Post cần cả Quận/Huyện.
    // Dò danh mục VTP: khớp Tỉnh theo tên, rồi duyệt Quận/Huyện tìm Phường/Xã khớp tên.
    private async Task<(int provinceId, int districtId, int wardId)> ResolveReceiverAsync(Order order, CancellationToken ct)
    {
        var provinces = await _client.GetProvincesAsync(ct);
        var prov = MatchByName(provinces, p => p.ProvinceName, order.ShippingProvinceName);
        if (prov?.ProvinceId is not int pid)
            throw new InvalidOperationException($"Không khớp Tỉnh/Thành '{order.ShippingProvinceName}' với danh mục Viettel Post.");

        if (string.IsNullOrWhiteSpace(order.ShippingWardName))
            throw new InvalidOperationException("Đơn hàng thiếu Phường/Xã — không xác định được mã địa chỉ Viettel Post.");

        var districts = await _client.GetDistrictsAsync(pid, ct);
        foreach (var d in districts)
        {
            if (d.DistrictId is not int did) continue;
            var wards = await _client.GetWardsAsync(did, ct);
            var w = MatchByName(wards, x => x.WardsName, order.ShippingWardName);
            if (w?.WardsId is int wid) return (pid, did, wid);
        }

        throw new InvalidOperationException(
            $"Không khớp Phường/Xã '{order.ShippingWardName}' với danh mục Viettel Post trong tỉnh '{order.ShippingProvinceName}'.");
    }

    private static VtpCategory? MatchByName(IEnumerable<VtpCategory> list, Func<VtpCategory, string?> selector, string? target)
    {
        var key = Normalize(target);
        if (key.Length == 0) return null;
        VtpCategory? partial = null;
        foreach (var item in list)
        {
            var name = Normalize(selector(item));
            if (name == key) return item;
            if (partial == null && name.Length > 0 && (name.EndsWith(key) || key.EndsWith(name)))
                partial = item;
        }
        return partial;
    }

    // Bỏ tiền tố hành chính + dấu cách thừa để so khớp mềm ("Phường 1" ~ "1", "Thành phố Huế" ~ "Huế").
    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var v = s.Trim().ToLower(CultureInfo.CurrentCulture);
        foreach (var prefix in new[] { "thành phố ", "tỉnh ", "quận ", "huyện ", "thị xã ", "phường ", "xã ", "thị trấn " })
            if (v.StartsWith(prefix)) { v = v[prefix.Length..]; break; }
        return string.Join(' ', v.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    // Khối lượng: 2 lạng = 1 áo → 200g/áo. Tổng = số áo × 200g.
    private int TotalQty(Order order)
    {
        var totalQty = order.Items?.Sum(i => i.Quantity) ?? 0;
        return totalQty <= 0 ? 1 : totalQty;
    }

    private int EstimateWeightGram(Order order) => TotalQty(order) * _opts.Defaults.WeightPerShirtGram;

    private List<VtpItem> BuildItems(Order order)
    {
        var result = new List<VtpItem>();
        if (order.Items == null || order.Items.Count == 0)
        {
            result.Add(new VtpItem { ProductName = order.OrderNumber, ProductQuantity = 1, ProductWeight = _opts.Defaults.WeightPerShirtGram });
            return result;
        }
        foreach (var it in order.Items)
        {
            result.Add(new VtpItem
            {
                ProductName = BuildItemName(it),
                ProductQuantity = it.Quantity,
                ProductPrice = it.UnitPrice,
                ProductWeight = Math.Max(1, it.Quantity) * _opts.Defaults.WeightPerShirtGram  // khối lượng theo số áo của dòng
            });
        }
        return result;
    }

    private static string BuildProductName(Order order)
    {
        var first = order.Items?.FirstOrDefault();
        return first != null ? BuildItemName(first) : "Đồng phục";
    }

    private static string BuildItemName(OrderItem it)
    {
        var parts = new[] { it.CollectionName, it.Description, it.Size }.Where(s => !string.IsNullOrWhiteSpace(s));
        var name = string.Join(" - ", parts);
        return string.IsNullOrWhiteSpace(name) ? "Đồng phục" : name;
    }

    private static string BuildTrackingUrl(string? orderNumber) =>
        $"https://viettelpost.com.vn/tra-cuu-hanh-trinh-don/?peada={orderNumber}";

    // Đã giao thành công (map gần đúng — điều chỉnh theo bảng mã Viettel Post của tài khoản).
    private static bool IsDeliveredStatus(int code) => code is 500 or 501;
    private static bool IsInTransitStatus(int code) => code is 200 or 201 or 300 or 301 or 400 or 401;

    private static string? MapStatusCode(int? code) => code switch
    {
        -100 => "chờ duyệt",
        100 => "đã tiếp nhận",
        102 => "đã duyệt",
        200 => "đã lấy hàng",
        300 => "đang vận chuyển",
        400 => "đến bưu cục phát",
        500 => "đang giao",
        501 => "đã giao",
        502 => "giao không thành công",
        107 => "đã huỷ",
        _ => null
    };

    private static ViettelPostShipmentDto ToDto(Order order) => new()
    {
        Label = order.ViettelPostLabel,
        TrackingUrl = order.ViettelPostTrackingUrl,
        Status = order.ViettelPostStatus,
        StatusCode = order.ViettelPostStatusCode,
        Fee = order.ViettelPostFee,
        InsuranceFee = order.ViettelPostInsuranceFee,
        SyncedAt = order.ViettelPostSyncedAt
    };
}
