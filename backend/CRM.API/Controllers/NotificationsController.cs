using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NotificationDto>>>> GetMyNotifications(
        [FromQuery] NotificationFilterDto filter)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetForUserAsync(userId, filter);
        return Ok(ApiResponse<PaginatedResult<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<ApiResponse>> MarkRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _notificationService.MarkReadAsync(id, userId);
        if (!ok)
        {
            return NotFound(ApiResponse.Fail("Không tìm thấy thông báo."));
        }
        return Ok(ApiResponse.Ok("Đã đánh dấu đã đọc."));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.MarkAllReadAsync(userId);
        return Ok(ApiResponse<int>.Ok(count, $"Đã đánh dấu {count} thông báo là đã đọc."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _notificationService.DeleteAsync(id, userId);
        if (!ok)
        {
            return NotFound(ApiResponse.Fail("Không tìm thấy thông báo."));
        }
        return Ok(ApiResponse.Ok("Đã xoá thông báo."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
