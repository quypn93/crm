namespace CRM.Infrastructure.Services.Ghtk;

// Low-level HTTP wrapper. Throw GhtkException nếu API trả lỗi hoặc chưa cấu hình.
public interface IGhtkClient
{
    bool IsConfigured { get; }
    Task<GhtkFeeResponse> GetFeeAsync(GhtkFeeQuery query, CancellationToken ct = default);
    Task<GhtkOrderResponse> CreateOrderAsync(GhtkCreateOrderRequest request, CancellationToken ct = default);
    Task<bool> CancelOrderAsync(string label, CancellationToken ct = default);
    Task<GhtkStatusResponse?> GetStatusAsync(string label, CancellationToken ct = default);
}

public class GhtkException : Exception
{
    public int? ErrorCode { get; }
    public GhtkException(string message, int? errorCode = null, Exception? inner = null)
        : base(message, inner) { ErrorCode = errorCode; }
}
