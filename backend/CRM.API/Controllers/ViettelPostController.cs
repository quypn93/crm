using CRM.Application.DTOs.Common;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Services.ViettelPost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/viettelpost")]
public class ViettelPostController : ControllerBase
{
    private readonly IViettelPostShipmentService _svc;
    private readonly IViettelPostClient _client;
    private readonly ViettelPostOptions _opts;
    private readonly ILogger<ViettelPostController> _log;

    public ViettelPostController(IViettelPostShipmentService svc, IViettelPostClient client,
        IOptions<ViettelPostOptions> opts, ILogger<ViettelPostController> log)
    {
        _svc = svc;
        _client = client;
        _opts = opts.Value;
        _log = log;
    }

    [HttpGet("status")]
    [Authorize]
    public ActionResult<ApiResponse<object>> GetStatus()
        => Ok(ApiResponse<object>.Ok(new { configured = _svc.IsConfigured }));

    // Danh mục hành chính Viettel Post — cho dropdown chọn kho gửi ở admin.
    [HttpGet("provinces")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<VtpCategory>>>> GetProvinces(CancellationToken ct)
        => Ok(ApiResponse<List<VtpCategory>>.Ok(await _client.GetProvincesAsync(ct)));

    [HttpGet("districts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<VtpCategory>>>> GetDistricts([FromQuery] int provinceId, CancellationToken ct)
        => Ok(ApiResponse<List<VtpCategory>>.Ok(await _client.GetDistrictsAsync(provinceId, ct)));

    [HttpGet("wards")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<VtpCategory>>>> GetWards([FromQuery] int districtId, CancellationToken ct)
        => Ok(ApiResponse<List<VtpCategory>>.Ok(await _client.GetWardsAsync(districtId, ct)));

    [HttpPost("orders/{orderId}/estimate-fee")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ViettelPostFeeDto>>> EstimateFee(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<ViettelPostFeeDto>.Fail("Viettel Post chưa cấu hình."));
        var fee = await _svc.EstimateFeeAsync(orderId, ct);
        return Ok(ApiResponse<ViettelPostFeeDto>.Ok(fee));
    }

    [HttpPost("orders/{orderId}/create")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ViettelPostShipmentDto>>> Create(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<ViettelPostShipmentDto>.Fail("Viettel Post chưa cấu hình."));
        var dto = await _svc.CreateShipmentAsync(orderId, ct);
        return Ok(ApiResponse<ViettelPostShipmentDto>.Ok(dto, "Tạo vận đơn Viettel Post thành công."));
    }

    [HttpPost("orders/{orderId}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse.Fail("Viettel Post chưa cấu hình."));
        var ok = await _svc.CancelShipmentAsync(orderId, ct);
        return ok
            ? Ok(ApiResponse.Ok("Đã huỷ vận đơn Viettel Post."))
            : BadRequest(ApiResponse.Fail("Huỷ vận đơn không thành công."));
    }

    [HttpPost("orders/{orderId}/sync")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ViettelPostShipmentDto>>> Sync(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<ViettelPostShipmentDto>.Fail("Viettel Post chưa cấu hình."));
        var dto = await _svc.SyncStatusAsync(orderId, ct);
        return Ok(ApiResponse<ViettelPostShipmentDto>.Ok(dto!));
    }

    // Viettel Post gọi POST callback mỗi khi đơn đổi trạng thái (cấu hình URL + secret ở cổng đối tác).
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromBody] VtpWebhookPayload payload,
        [FromHeader(Name = "X-VTP-Secret")] string? secret,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_opts.WebhookSecret) && !string.Equals(secret, _opts.WebhookSecret, StringComparison.Ordinal))
        {
            _log.LogWarning("Viettel Post webhook secret mismatch. Received={Received}", secret);
            return Unauthorized(new { error = "Invalid secret" });
        }

        if (string.IsNullOrWhiteSpace(payload.OrderNumber) || payload.OrderStatus == null)
            return BadRequest(new { error = "Payload thiếu ORDER_NUMBER/ORDER_STATUS" });

        await _svc.HandleWebhookAsync(payload.OrderNumber, payload.OrderStatus.Value, payload.StatusName, payload.MoneyTotal, ct);
        return Ok(new { success = true });
    }
}
