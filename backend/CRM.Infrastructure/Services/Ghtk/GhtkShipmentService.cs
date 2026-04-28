using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Ghtk;

public class GhtkShipmentService : IGhtkShipmentService
{
    private readonly IGhtkClient _client;
    private readonly GhtkOptions _opts;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GhtkShipmentService> _log;

    public GhtkShipmentService(IGhtkClient client, IOptions<GhtkOptions> opts, IUnitOfWork uow, ILogger<GhtkShipmentService> log)
    {
        _client = client;
        _opts = opts.Value;
        _uow = uow;
        _log = log;
    }

    public bool IsConfigured => _client.IsConfigured;

    public async Task<GhtkFeeDto> EstimateFeeAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        ValidateAddress(order);

        var weight = EstimateWeightKg(order);
        var query = new GhtkFeeQuery
        {
            PickProvince = _opts.Pick.Province,
            PickDistrict = _opts.Pick.District,
            Province = order.ShippingProvinceName ?? string.Empty,
            Ward = order.ShippingWardName,
            Address = order.ShippingAddress,
            Weight = weight,
            Value = order.TotalAmount,
            Transport = _opts.Defaults.Transport
        };

        var fee = await _client.GetFeeAsync(query, ct);

        order.GhtkFee = fee.Fee;
        order.GhtkInsuranceFee = fee.InsuranceFee;
        order.GhtkSyncedAt = DateTime.UtcNow;
        order.GhtkLastError = null;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return new GhtkFeeDto
        {
            Fee = fee.Fee ?? 0,
            InsuranceFee = fee.InsuranceFee ?? 0,
            DeliveryType = fee.DeliveryType
        };
    }

    public async Task<GhtkShipmentDto> CreateShipmentAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.DeliveryMethod != DeliveryMethod.GHTK)
            throw new InvalidOperationException("Đơn hàng không chọn hình thức GHTK.");

        if (!string.IsNullOrWhiteSpace(order.GhtkLabel))
            throw new InvalidOperationException($"Đơn đã có vận đơn GHTK {order.GhtkLabel}.");

        ValidateAddress(order);

        var products = BuildProducts(order);
        var req = new GhtkCreateOrderRequest
        {
            Products = products,
            Order = new GhtkOrderPayload
            {
                Id = order.OrderNumber,
                PickName = _opts.Pick.Name,
                PickTel = _opts.Pick.Tel,
                PickAddress = _opts.Pick.Address,
                PickProvince = _opts.Pick.Province,
                PickDistrict = _opts.Pick.District,
                PickWard = _opts.Pick.Ward,
                PickMoney = _opts.Defaults.UseCod ? order.TotalAmount - order.PaidAmount : 0,
                Tel = order.ShippingPhone ?? string.Empty,
                Name = order.ShippingContactName ?? order.CustomerName ?? string.Empty,
                Address = order.ShippingAddress ?? string.Empty,
                Province = order.ShippingProvinceName ?? string.Empty,
                Ward = order.ShippingWardName,
                Value = order.TotalAmount,
                Transport = _opts.Defaults.Transport,
                PickWorkShift = _opts.Defaults.PickWorkShift,
                IsFreeship = _opts.Defaults.UseCod ? 0 : 1,
                Note = order.ShippingNotes
            }
        };

        try
        {
            var resp = await _client.CreateOrderAsync(req, ct);

            order.GhtkLabel = resp.Label;
            order.GhtkTrackingUrl = resp.TrackingUrl;
            order.GhtkStatusCode = resp.StatusId;
            order.GhtkStatus = MapStatusCode(resp.StatusId);
            order.GhtkFee = resp.Fee;
            order.GhtkInsuranceFee = resp.InsuranceFee;
            order.GhtkSyncedAt = DateTime.UtcNow;
            order.GhtkLastError = null;

            if (_opts.Defaults.AutoOverrideShippingFee && resp.Fee.HasValue)
            {
                // Nếu policy auto-override, cộng phí GHTK vào TotalAmount.
                // Tránh cộng trùng: chỉ set khi chưa có.
                _log.LogInformation("GHTK auto-override fee {Fee} on order {OrderId}", resp.Fee, orderId);
            }

            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();

            return ToDto(order);
        }
        catch (GhtkException ex)
        {
            order.GhtkLastError = ex.Message;
            order.GhtkSyncedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
            throw;
        }
    }

    public async Task<bool> CancelShipmentAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (string.IsNullOrWhiteSpace(order.GhtkLabel))
            throw new InvalidOperationException("Đơn chưa có vận đơn GHTK.");

        var ok = await _client.CancelOrderAsync(order.GhtkLabel, ct);
        if (ok)
        {
            order.GhtkStatus = "cancelled";
            order.GhtkStatusCode = 45;
            order.GhtkSyncedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
        }
        return ok;
    }

    public async Task<GhtkShipmentDto?> SyncStatusAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        if (string.IsNullOrWhiteSpace(order.GhtkLabel)) return null;

        var status = await _client.GetStatusAsync(order.GhtkLabel, ct);
        if (status == null) return null;

        order.GhtkStatus = status.Status;
        order.GhtkSyncedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task HandleWebhookAsync(string label, int statusCode, string? statusText, decimal? fee, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.GhtkLabel == label);
        if (order == null)
        {
            _log.LogWarning("GHTK webhook nhận label không khớp order: {Label}", label);
            return;
        }

        order.GhtkStatusCode = statusCode;
        order.GhtkStatus = statusText ?? MapStatusCode(statusCode);
        if (fee.HasValue) order.GhtkFee = fee.Value;
        order.GhtkSyncedAt = DateTime.UtcNow;

        // Tự động đồng bộ trạng thái đơn của mình theo GHTK.
        switch (statusCode)
        {
            case 5:  // đã giao
            case 6:  // đã đối soát
                if (order.Status == OrderStatus.Shipping)
                {
                    order.Status = OrderStatus.Delivered;
                    order.ActualDeliveryDate = DateTime.UtcNow;
                }
                break;
            case 4:  // đang giao
                if (order.Status == OrderStatus.ReadyToShip)
                    order.Status = OrderStatus.Shipping;
                break;
        }

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();
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
            throw new InvalidOperationException("GHTK chưa cấu hình — không thể tạo vận đơn.");
    }

    private decimal EstimateWeightKg(Order order)
    {
        var totalQty = order.Items?.Sum(i => i.Quantity) ?? 0;
        if (totalQty <= 0) totalQty = 1;
        return Math.Max(_opts.Defaults.DefaultWeightKg, totalQty * _opts.Defaults.DefaultWeightKg);
    }

    private List<GhtkProduct> BuildProducts(Order order)
    {
        var result = new List<GhtkProduct>();
        if (order.Items == null || order.Items.Count == 0)
        {
            result.Add(new GhtkProduct
            {
                Name = order.OrderNumber,
                Quantity = 1,
                Weight = _opts.Defaults.DefaultWeightKg
            });
            return result;
        }

        foreach (var it in order.Items)
        {
            result.Add(new GhtkProduct
            {
                Name = BuildProductName(it),
                Quantity = it.Quantity,
                Weight = _opts.Defaults.DefaultWeightKg,
                ProductCode = it.ProductCode
            });
        }
        return result;
    }

    private static string BuildProductName(OrderItem it)
    {
        var parts = new[] { it.CollectionName, it.Description, it.Size }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        var name = string.Join(" - ", parts);
        return string.IsNullOrWhiteSpace(name) ? "Đồng phục" : name;
    }

    // Map subset status code → label tiếng Việt. Full spec tại docs GHTK.
    private static string? MapStatusCode(int? code) => code switch
    {
        -1 => "pending",
        1 => "chưa tiếp nhận",
        2 => "đã tiếp nhận",
        3 => "đang lấy hàng",
        4 => "đang vận chuyển",
        5 => "đã giao",
        6 => "đã đối soát",
        7 => "không lấy được hàng",
        8 => "hoãn lấy hàng",
        9 => "không giao được",
        10 => "delay giao hàng",
        11 => "đã đối soát công nợ",
        13 => "shop/NGV lấy hàng",
        20 => "đang chuyển hoàn",
        21 => "đã chuyển hoàn",
        45 => "huỷ đơn",
        _ => null
    };

    private static GhtkShipmentDto ToDto(Order order) => new()
    {
        Label = order.GhtkLabel,
        TrackingUrl = order.GhtkTrackingUrl,
        Status = order.GhtkStatus,
        StatusCode = order.GhtkStatusCode,
        Fee = order.GhtkFee,
        InsuranceFee = order.GhtkInsuranceFee,
        SyncedAt = order.GhtkSyncedAt
    };
}
