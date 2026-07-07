namespace CRM.Infrastructure.Services.ViettelPost;

public interface IViettelPostClient
{
    bool IsConfigured { get; }
    Task<VtpPriceData> GetFeeAsync(VtpFeeQuery query, CancellationToken ct = default);
    Task<VtpCreateOrderData> CreateOrderAsync(VtpCreateOrderRequest req, CancellationToken ct = default);
    Task<bool> CancelOrderAsync(string orderNumber, string? note, CancellationToken ct = default);
    Task<VtpOrderStatusData?> GetStatusAsync(string orderNumber, CancellationToken ct = default);

    Task<List<VtpCategory>> GetProvincesAsync(CancellationToken ct = default);
    Task<List<VtpCategory>> GetDistrictsAsync(int provinceId, CancellationToken ct = default);
    Task<List<VtpCategory>> GetWardsAsync(int districtId, CancellationToken ct = default);
}

public class ViettelPostException : Exception
{
    public int? StatusCode { get; }
    public ViettelPostException(string message, int? statusCode = null) : base(message) => StatusCode = statusCode;
}
