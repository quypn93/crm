namespace CRM.Application.Interfaces;

public interface IGhtkShipmentService
{
    bool IsConfigured { get; }

    // Trả về phí ước tính (đã ghi vào Order.GhtkFee nếu orderId có giá trị).
    Task<GhtkFeeDto> EstimateFeeAsync(Guid orderId, CancellationToken ct = default);

    // Tạo vận đơn GHTK. Cập nhật Order.GhtkLabel/GhtkStatus/... và trả Order đã mới.
    Task<GhtkShipmentDto> CreateShipmentAsync(Guid orderId, CancellationToken ct = default);

    // Huỷ vận đơn GHTK đã tạo.
    Task<bool> CancelShipmentAsync(Guid orderId, CancellationToken ct = default);

    // Đồng bộ trạng thái từ GHTK về DB (call định kỳ hoặc khi user refresh).
    Task<GhtkShipmentDto?> SyncStatusAsync(Guid orderId, CancellationToken ct = default);

    // Handler cho webhook — label + statusCode từ GHTK gửi về.
    Task HandleWebhookAsync(string label, int statusCode, string? statusText, decimal? fee, CancellationToken ct = default);
}

public class GhtkFeeDto
{
    public decimal Fee { get; set; }
    public decimal InsuranceFee { get; set; }
    public string? DeliveryType { get; set; }
}

public class GhtkShipmentDto
{
    public string? Label { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Status { get; set; }
    public int? StatusCode { get; set; }
    public decimal? Fee { get; set; }
    public decimal? InsuranceFee { get; set; }
    public DateTime? SyncedAt { get; set; }
}
