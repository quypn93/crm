using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Notification;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/admin/notification-preferences")]
[Authorize(Roles = RoleNames.Admin)]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferenceService _preferenceService;

    public NotificationPreferencesController(INotificationPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationRolePreferenceDto>>>> GetAll()
    {
        var prefs = await _preferenceService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<NotificationRolePreferenceDto>>.Ok(prefs));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> Update([FromBody] UpdateRolePreferencesRequest request)
    {
        await _preferenceService.UpdateAsync(request);
        return Ok(ApiResponse.Ok("Đã cập nhật cấu hình thông báo."));
    }

    [HttpPost("reset")]
    public async Task<ActionResult<ApiResponse>> Reset()
    {
        await _preferenceService.ResetToDefaultsAsync();
        return Ok(ApiResponse.Ok("Đã khôi phục cấu hình mặc định."));
    }
}
