using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Domain.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetStats()
    {
        var userId = GetUserId();
        var stats = await _dashboardService.GetDashboardStatsAsync(userId);
        return Ok(ApiResponse<DashboardStats>.Ok(stats));
    }

    [HttpGet("case-chart")]
    public async Task<ActionResult<ApiResponse<List<CaseChartDataPoint>>>> GetCaseChart(
        [FromQuery] int months = 12)
    {
        var userId = GetUserId();
        var data = await _dashboardService.GetCaseChartDataAsync(userId, months);
        return Ok(ApiResponse<List<CaseChartDataPoint>>.Ok(data));
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<ApiResponse<List<RecentActivity>>>> GetRecentActivities(
        [FromQuery] int count = 10)
    {
        var userId = GetUserId();
        var activities = await _dashboardService.GetRecentActivitiesAsync(userId, count);
        return Ok(ApiResponse<List<RecentActivity>>.Ok(activities));
    }
}
