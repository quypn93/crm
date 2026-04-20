using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Production;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Authorize]
public class OrderProductionController : ControllerBase
{
    private readonly IOrderProductionService _service;

    public OrderProductionController(IOrderProductionService service)
    {
        _service = service;
    }

    // ── Dashboard sản xuất ───────────────────────────────────────────

    /// <summary>Danh sách tất cả đơn đang sản xuất kèm tiến độ</summary>
    [HttpGet("api/production/dashboard")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager},{RoleNames.QualityControl}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderProductionProgressDto>>>> GetDashboard()
    {
        var list = await _service.GetAllInProductionAsync();
        return Ok(ApiResponse<IEnumerable<OrderProductionProgressDto>>.Ok(list));
    }

    // ── Truy cập theo orderId (desktop) ──────────────────────────────

    /// <summary>Lấy tiến độ sản xuất của một đơn</summary>
    [HttpGet("api/orders/{orderId:guid}/production")]
    public async Task<ActionResult<ApiResponse<OrderProductionProgressDto>>> GetProgress(Guid orderId)
    {
        try
        {
            var progress = await _service.GetProgressAsync(orderId);
            return Ok(ApiResponse<OrderProductionProgressDto>.Ok(progress));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderProductionProgressDto>.Fail(ex.Message));
        }
    }

    /// <summary>Hoàn thành một bước sản xuất (desktop)</summary>
    [HttpPost("api/orders/{orderId:guid}/production/steps/{stageId:guid}/complete")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager},{RoleNames.ProductionStaff}," +
                       $"{RoleNames.CuttingStaff},{RoleNames.SewingStaff},{RoleNames.PrintingStaff}," +
                       $"{RoleNames.FinishingStaff},{RoleNames.PackagingStaff}," +
                       $"{RoleNames.QualityControl},{RoleNames.QualityManager}")]
    public async Task<ActionResult<ApiResponse<OrderProductionStepDto>>> CompleteStep(
        Guid orderId, Guid stageId, [FromBody] CompleteProductionStepDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var step = await _service.CompleteStepAsync(orderId, stageId, userId, dto);
            return Ok(ApiResponse<OrderProductionStepDto>.Ok(step));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
    }

    // ── Truy cập qua QR token (mobile) ───────────────────────────────

    /// <summary>Lấy tiến độ sản xuất qua QR token (mobile scan)</summary>
    [HttpGet("api/production/scan/{token}")]
    public async Task<ActionResult<ApiResponse<OrderProductionProgressDto>>> GetProgressByToken(string token)
    {
        try
        {
            var progress = await _service.GetProgressByTokenAsync(token);
            return Ok(ApiResponse<OrderProductionProgressDto>.Ok(progress));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderProductionProgressDto>.Fail(ex.Message));
        }
    }

    /// <summary>Hoàn thành bước sản xuất qua QR token (mobile scan)</summary>
    [HttpPost("api/production/scan/{token}/steps/{stageId:guid}/complete")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager},{RoleNames.ProductionStaff}," +
                       $"{RoleNames.CuttingStaff},{RoleNames.SewingStaff},{RoleNames.PrintingStaff}," +
                       $"{RoleNames.FinishingStaff},{RoleNames.PackagingStaff}," +
                       $"{RoleNames.QualityControl},{RoleNames.QualityManager}")]
    public async Task<ActionResult<ApiResponse<OrderProductionStepDto>>> CompleteStepByToken(
        string token, Guid stageId, [FromBody] CompleteProductionStepDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var step = await _service.CompleteStepByTokenAsync(token, stageId, userId, dto);
            return Ok(ApiResponse<OrderProductionStepDto>.Ok(step));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<OrderProductionStepDto>.Fail(ex.Message)); }
    }

    // ─────────────────────────────────────────────────────────────────
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Không xác định được người dùng.");
        return Guid.Parse(claim);
    }
}
