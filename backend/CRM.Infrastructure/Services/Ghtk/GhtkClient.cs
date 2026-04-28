using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Ghtk;

public class GhtkClient : IGhtkClient
{
    private readonly HttpClient _http;
    private readonly GhtkOptions _opts;
    private readonly ILogger<GhtkClient> _log;

    public GhtkClient(HttpClient http, IOptions<GhtkOptions> opts, ILogger<GhtkClient> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;

        if (IsConfigured)
        {
            _http.BaseAddress = new Uri(_opts.BaseUrl.TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Remove("Token");
            _http.DefaultRequestHeaders.Add("Token", _opts.Token);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(_opts.PartnerCode))
            {
                _http.DefaultRequestHeaders.Remove("X-Client-Source");
                _http.DefaultRequestHeaders.Add("X-Client-Source", _opts.PartnerCode);
            }
        }
    }

    public bool IsConfigured => _opts.IsConfigured;

    public async Task<GhtkFeeResponse> GetFeeAsync(GhtkFeeQuery q, CancellationToken ct = default)
    {
        EnsureConfigured();

        var qs = HttpUtility.ParseQueryString(string.Empty);
        qs["pick_province"] = q.PickProvince;
        qs["pick_district"] = q.PickDistrict;
        qs["province"] = q.Province;
        if (!string.IsNullOrWhiteSpace(q.District)) qs["district"] = q.District;
        if (!string.IsNullOrWhiteSpace(q.Ward)) qs["deliver_ward"] = q.Ward;
        if (!string.IsNullOrWhiteSpace(q.Address)) qs["address"] = q.Address;
        qs["weight"] = (q.Weight * 1000).ToString("0"); // GHTK nhận gram
        qs["value"] = ((int)q.Value).ToString();
        qs["transport"] = q.Transport;

        var url = $"services/shipment/fee?{qs}";
        var resp = await _http.GetAsync(url, ct);
        var payload = await ReadApiResponseAsync<GhtkFeeResponse>(resp, ct);
        return payload.Fee ?? throw new GhtkException(payload.Message ?? "Không lấy được phí GHTK.", payload.ErrorCode);
    }

    public async Task<GhtkOrderResponse> CreateOrderAsync(GhtkCreateOrderRequest request, CancellationToken ct = default)
    {
        EnsureConfigured();

        var resp = await _http.PostAsJsonAsync("services/shipment/order", request, ct);
        var payload = await ReadApiResponseAsync<GhtkOrderResponse>(resp, ct);
        return payload.Order ?? throw new GhtkException(payload.Message ?? "Tạo vận đơn GHTK thất bại.", payload.ErrorCode);
    }

    public async Task<bool> CancelOrderAsync(string label, CancellationToken ct = default)
    {
        EnsureConfigured();
        var resp = await _http.PostAsync($"services/shipment/cancel/{label}", content: null, ct);
        var payload = await ReadApiResponseAsync<object>(resp, ct);
        return payload.Success;
    }

    public async Task<GhtkStatusResponse?> GetStatusAsync(string label, CancellationToken ct = default)
    {
        EnsureConfigured();
        var resp = await _http.GetAsync($"services/shipment/v2/{label}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var payload = await ReadApiResponseAsync<GhtkStatusResponse>(resp, ct);
        return payload.Order;
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new GhtkException("GHTK chưa cấu hình. Vui lòng điền Token và thông tin kho trong appsettings:Ghtk.");
    }

    private async Task<GhtkApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new GhtkException($"GHTK trả response trống (HTTP {(int)resp.StatusCode}).");
        }

        GhtkApiResponse<T>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GhtkApiResponse<T>>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _log.LogError(ex, "GHTK response parse error. Body={Body}", body);
            throw new GhtkException($"Không parse được response GHTK: {body}", null, ex);
        }

        if (parsed == null)
            throw new GhtkException("Response GHTK không hợp lệ.");

        if (!parsed.Success && !resp.IsSuccessStatusCode)
        {
            throw new GhtkException(parsed.Message ?? $"GHTK HTTP {(int)resp.StatusCode}.", parsed.ErrorCode);
        }

        return parsed;
    }
}
