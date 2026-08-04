using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Report;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Bảng xếp hạng hiệu suất — mọi tài khoản đăng nhập đều xem được, không giới hạn vai trò.
[ApiController]
[Route("api/leaderboard")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _svc;
    public LeaderboardController(ILeaderboardService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<LeaderboardResultDto>>> Get(
        [FromQuery] LeaderboardScope scope = LeaderboardScope.Sales,
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Month,
        [FromQuery] DateTime? date = null)
    {
        var result = await _svc.GetLeaderboardAsync(scope, period, date ?? DateTime.UtcNow);
        return Ok(ApiResponse<LeaderboardResultDto>.Ok(result));
    }
}
