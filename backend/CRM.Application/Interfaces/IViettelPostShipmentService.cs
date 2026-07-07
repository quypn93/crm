namespace CRM.Application.Interfaces;

public interface IViettelPostShipmentService
{
    bool IsConfigured { get; }
    Task<ViettelPostFeeDto> EstimateFeeAsync(Guid orderId, CancellationToken ct = default);
    Task<ViettelPostShipmentDto> CreateShipmentAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> CancelShipmentAsync(Guid orderId, CancellationToken ct = default);
    Task<ViettelPostShipmentDto?> SyncStatusAsync(Guid orderId, CancellationToken ct = default);
    Task HandleWebhookAsync(string orderNumber, int statusCode, string? statusText, decimal? fee, CancellationToken ct = default);
}

public class ViettelPostFeeDto
{
    public decimal Fee { get; set; }
    public decimal InsuranceFee { get; set; }
    public string? DeliveryType { get; set; }
}

public class ViettelPostShipmentDto
{
    public string? Label { get; set; }
    public string? TrackingUrl { get; set; }
    public string? Status { get; set; }
    public int? StatusCode { get; set; }
    public decimal? Fee { get; set; }
    public decimal? InsuranceFee { get; set; }
    public DateTime? SyncedAt { get; set; }
}
