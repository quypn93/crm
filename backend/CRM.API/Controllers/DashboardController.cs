using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Report;
using CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboard()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }

    [HttpGet("my-stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetMyStats()
    {
        var userId = GetCurrentUserId();
        var stats = await _dashboardService.GetDashboardStatsAsync(userId);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RevenueReportDto>>>> GetRevenueReport([FromQuery] ReportFilterDto filter)
    {
        var report = await _dashboardService.GetRevenueReportAsync(filter);
        return Ok(ApiResponse<IEnumerable<RevenueReportDto>>.Ok(report));
    }

    [HttpGet("deals-by-stage")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DealsByStageReportDto>>>> GetDealsByStage()
    {
        var report = await _dashboardService.GetDealsByStageReportAsync();
        return Ok(ApiResponse<IEnumerable<DealsByStageReportDto>>.Ok(report));
    }

    [HttpGet("customers-by-industry")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomersByIndustryReportDto>>>> GetCustomersByIndustry()
    {
        var report = await _dashboardService.GetCustomersByIndustryReportAsync();
        return Ok(ApiResponse<IEnumerable<CustomersByIndustryReportDto>>.Ok(report));
    }

    [HttpGet("sales-performance")]
    [Authorize(Roles = "Admin,SalesManager")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalesPerformanceDto>>>> GetSalesPerformance([FromQuery] ReportFilterDto filter)
    {
        var report = await _dashboardService.GetSalesPerformanceAsync(filter);
        return Ok(ApiResponse<IEnumerable<SalesPerformanceDto>>.Ok(report));
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ActivityLogDto>>>> GetRecentActivities([FromQuery] int count = 10)
    {
        var activities = await _dashboardService.GetRecentActivitiesAsync(count);
        return Ok(ApiResponse<IEnumerable<ActivityLogDto>>.Ok(activities));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
