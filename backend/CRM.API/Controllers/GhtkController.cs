using CRM.Application.DTOs.Common;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Services.Ghtk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/ghtk")]
public class GhtkController : ControllerBase
{
    private readonly IGhtkShipmentService _svc;
    private readonly GhtkOptions _opts;
    private readonly ILogger<GhtkController> _log;

    public GhtkController(IGhtkShipmentService svc, IOptions<GhtkOptions> opts, ILogger<GhtkController> log)
    {
        _svc = svc;
        _opts = opts.Value;
        _log = log;
    }

    [HttpGet("status")]
    [Authorize]
    public ActionResult<ApiResponse<object>> GetStatus()
        => Ok(ApiResponse<object>.Ok(new { configured = _svc.IsConfigured }));

    [HttpPost("orders/{orderId}/estimate-fee")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GhtkFeeDto>>> EstimateFee(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<GhtkFeeDto>.Fail("GHTK chưa cấu hình."));
        var fee = await _svc.EstimateFeeAsync(orderId, ct);
        return Ok(ApiResponse<GhtkFeeDto>.Ok(fee));
    }

    [HttpPost("orders/{orderId}/create")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GhtkShipmentDto>>> Create(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<GhtkShipmentDto>.Fail("GHTK chưa cấu hình."));
        var dto = await _svc.CreateShipmentAsync(orderId, ct);
        return Ok(ApiResponse<GhtkShipmentDto>.Ok(dto, "Tạo vận đơn GHTK thành công."));
    }

    [HttpPost("orders/{orderId}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse.Fail("GHTK chưa cấu hình."));
        var ok = await _svc.CancelShipmentAsync(orderId, ct);
        return ok
            ? Ok(ApiResponse.Ok("Đã huỷ vận đơn GHTK."))
            : BadRequest(ApiResponse.Fail("Huỷ vận đơn không thành công."));
    }

    [HttpPost("orders/{orderId}/sync")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GhtkShipmentDto>>> Sync(Guid orderId, CancellationToken ct)
    {
        if (!_svc.IsConfigured)
            return StatusCode(503, ApiResponse<GhtkShipmentDto>.Fail("GHTK chưa cấu hình."));
        var dto = await _svc.SyncStatusAsync(orderId, ct);
        return Ok(ApiResponse<GhtkShipmentDto>.Ok(dto!));
    }

    // GHTK gửi POST callback mỗi khi đơn đổi trạng thái.
    // Cấu hình URL + secret ở dashboard GHTK. Tham chiếu: docs GHTK "Tracking API".
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromBody] GhtkWebhookPayload payload,
        [FromHeader(Name = "X-GHTK-Secret")] string? secret,
        CancellationToken ct)
    {
        // Xác thực secret nếu đã cấu hình.
        if (!string.IsNullOrEmpty(_opts.WebhookSecret) && !string.Equals(secret, _opts.WebhookSecret, StringComparison.Ordinal))
        {
            _log.LogWarning("GHTK webhook secret mismatch. Received={Received}", secret);
            return Unauthorized(new { error = "Invalid secret" });
        }

        if (string.IsNullOrWhiteSpace(payload.LabelId) || payload.StatusId == null)
            return BadRequest(new { error = "Payload thiếu label_id/status_id" });

        await _svc.HandleWebhookAsync(payload.LabelId, payload.StatusId.Value, payload.Reason, payload.Fee, ct);
        return Ok(new { success = true });
    }
}
