using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.ViettelPost;

// Client HTTP mức thấp cho Viettel Post Open API v2.
// Auth: header "Token". Ưu tiên Token tĩnh trong config; nếu trống thì Login bằng Username/Password để lấy token (cache).
// Lưu ý: endpoint/tên trường có thể cần chỉnh theo tài khoản đối tác thực tế của Viettel Post.
public class ViettelPostClient : IViettelPostClient
{
    private readonly HttpClient _http;
    private readonly ViettelPostOptions _opts;
    private readonly ILogger<ViettelPostClient> _log;
    private string? _cachedToken;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ViettelPostClient(HttpClient http, IOptions<ViettelPostOptions> opts, ILogger<ViettelPostClient> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;

        if (!string.IsNullOrWhiteSpace(_opts.BaseUrl))
        {
            _http.BaseAddress = new Uri(_opts.BaseUrl.TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    public bool IsConfigured => _opts.IsConfigured;

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_opts.Token)) return _opts.Token;
        if (!string.IsNullOrWhiteSpace(_cachedToken)) return _cachedToken!;
        if (string.IsNullOrWhiteSpace(_opts.Username) || string.IsNullOrWhiteSpace(_opts.Password))
            throw new ViettelPostException("Viettel Post chưa cấu hình Token hoặc Username/Password.");

        var login = new VtpLoginRequest { Username = _opts.Username!, Password = _opts.Password! };
        using var resp = await _http.PostAsJsonAsync("user/login", login, ct);
        var data = await ReadDataAsync<VtpLoginData>(resp, ct);
        if (string.IsNullOrWhiteSpace(data?.Token))
            throw new ViettelPostException("Đăng nhập Viettel Post thất bại (không nhận được token).");
        _cachedToken = data!.Token;
        return _cachedToken!;
    }

    private async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        EnsureConfigured();
        var token = await GetTokenAsync(ct);
        var req = new HttpRequestMessage(method, path);
        req.Headers.TryAddWithoutValidation("Token", token);
        if (body != null) req.Content = JsonContent.Create(body);
        return await _http.SendAsync(req, ct);
    }

    public async Task<VtpPriceData> GetFeeAsync(VtpFeeQuery q, CancellationToken ct = default)
    {
        var body = new VtpPriceRequest
        {
            SenderProvince = q.SenderProvince,
            SenderDistrict = q.SenderDistrict,
            ReceiverProvince = q.ReceiverProvince,
            ReceiverDistrict = q.ReceiverDistrict,
            ProductType = _opts.Defaults.ProductType,
            ProductWeight = q.WeightGram,
            ProductPrice = q.Value,
            MoneyCollection = q.MoneyCollection,
            OrderService = q.OrderService ?? _opts.Defaults.OrderService,
            Type = 1
        };
        using var resp = await SendJsonAsync(HttpMethod.Post, "order/getPrice", body, ct);
        return await ReadDataAsync<VtpPriceData>(resp, ct)
            ?? throw new ViettelPostException("Không lấy được phí Viettel Post.");
    }

    public async Task<VtpCreateOrderData> CreateOrderAsync(VtpCreateOrderRequest req, CancellationToken ct = default)
    {
        using var resp = await SendJsonAsync(HttpMethod.Post, "order/createOrder", req, ct);
        return await ReadDataAsync<VtpCreateOrderData>(resp, ct)
            ?? throw new ViettelPostException("Tạo vận đơn Viettel Post thất bại.");
    }

    public async Task<bool> CancelOrderAsync(string orderNumber, string? note, CancellationToken ct = default)
    {
        var body = new VtpCancelRequest { Type = 4, OrderNumber = orderNumber, Note = note };
        using var resp = await SendJsonAsync(HttpMethod.Post, "order/UpdateOrder", body, ct);
        var wrapper = await ReadWrapperAsync<object>(resp, ct);
        return wrapper is { Error: false };
    }

    public async Task<VtpOrderStatusData?> GetStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        EnsureConfigured();
        var token = await GetTokenAsync(ct);
        var req = new HttpRequestMessage(HttpMethod.Get, $"order/getOrderDetail?orderNumber={Uri.EscapeDataString(orderNumber)}");
        req.Headers.TryAddWithoutValidation("Token", token);
        using var resp = await _http.SendAsync(req, ct);
        return await ReadDataAsync<VtpOrderStatusData>(resp, ct);
    }

    public async Task<List<VtpCategory>> GetProvincesAsync(CancellationToken ct = default)
    {
        using var resp = await SendJsonAsync(HttpMethod.Get, "categories/listProvince", null, ct);
        return await ReadDataAsync<List<VtpCategory>>(resp, ct) ?? new List<VtpCategory>();
    }

    public async Task<List<VtpCategory>> GetDistrictsAsync(int provinceId, CancellationToken ct = default)
    {
        using var resp = await SendJsonAsync(HttpMethod.Get, $"categories/listDistrict?provinceId={provinceId}", null, ct);
        return await ReadDataAsync<List<VtpCategory>>(resp, ct) ?? new List<VtpCategory>();
    }

    public async Task<List<VtpCategory>> GetWardsAsync(int districtId, CancellationToken ct = default)
    {
        using var resp = await SendJsonAsync(HttpMethod.Get, $"categories/listWards?districtId={districtId}", null, ct);
        return await ReadDataAsync<List<VtpCategory>>(resp, ct) ?? new List<VtpCategory>();
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new ViettelPostException("Viettel Post chưa cấu hình — không thể gọi API.");
    }

    private async Task<T?> ReadDataAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        var wrapper = await ReadWrapperAsync<T>(resp, ct);
        if (wrapper == null) return default;
        if (wrapper.Error)
            throw new ViettelPostException(wrapper.Message ?? "Viettel Post trả về lỗi.", wrapper.Status);
        return wrapper.Data;
    }

    private async Task<VtpApiResponse<T>?> ReadWrapperAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (!resp.IsSuccessStatusCode)
                throw new ViettelPostException($"Viettel Post HTTP {(int)resp.StatusCode}.", (int)resp.StatusCode);
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<VtpApiResponse<T>>(raw, JsonOpts);
        }
        catch (JsonException ex)
        {
            _log.LogWarning(ex, "Không parse được phản hồi Viettel Post: {Raw}", raw);
            throw new ViettelPostException("Không đọc được phản hồi Viettel Post.");
        }
    }
}
