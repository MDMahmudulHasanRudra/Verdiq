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
}
