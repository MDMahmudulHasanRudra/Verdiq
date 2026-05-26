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
    public async Task<ActionResult<ApiResponse<object>>> GetStats()
    {
        var chamberId = GetChamberId();
        var stats = await _dashboardService.GetStatsAsync(chamberId);
        return Ok(ApiResponse<object>.Ok(stats));
    }

    [HttpGet("case-chart")]
    public async Task<ActionResult<ApiResponse<object>>> GetCaseChart(
        [FromQuery] int months = 12)
    {
        var chamberId = GetChamberId();
        var data = await _dashboardService.GetCaseChartAsync(chamberId, months);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<ApiResponse<object>>> GetRecentActivities(
        [FromQuery] int count = 10)
    {
        var chamberId = GetChamberId();
        var activities = await _dashboardService.GetRecentActivitiesAsync(chamberId, count);
        return Ok(ApiResponse<object>.Ok(activities));
    }

    [HttpGet("lawyer-productivity")]
    public async Task<ActionResult<ApiResponse<object>>> GetLawyerProductivity()
    {
        var chamberId = GetChamberId();
        var data = await _dashboardService.GetLawyerProductivityAsync(chamberId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("win-ratio")]
    public async Task<ActionResult<ApiResponse<object>>> GetWinRatio()
    {
        var chamberId = GetChamberId();
        var data = await _dashboardService.GetWinRatioAsync(chamberId);
        return Ok(ApiResponse<object>.Ok(data));
    }
}
