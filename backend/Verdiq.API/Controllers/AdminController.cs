using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Admin;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetUsers(
        [FromQuery] string? search = null)
    {
        var users = await _adminService.GetUsersAsync();
        if (!string.IsNullOrWhiteSpace(search))
            users = users.Where(u =>
                u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        return Ok(ApiResponse<List<AdminUserDto>>.Ok(users.ToList()));
    }

    [HttpPatch("users/{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUserStatus(
        Guid id)
    {
        try
        {
            var (success, message) = await _adminService.ToggleUserStatusAsync(id);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));
            return Ok(ApiResponse<object>.Ok(null!, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
    {
        try
        {
            var (success, message) = await _adminService.ToggleUserStatusAsync(id);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));
            return Ok(ApiResponse<object>.Ok(null!, "User deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("cases")]
    public async Task<ActionResult<ApiResponse<List<AdminCaseDto>>>> GetCases()
    {
        var cases = await _adminService.GetCasesAsync();
        return Ok(ApiResponse<List<AdminCaseDto>>.Ok(cases.ToList()));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<List<AdminRevenueDto>>>> GetRevenue(
        [FromQuery] int months = 6)
    {
        var revenue = await _adminService.GetRevenueAsync(months);
        return Ok(ApiResponse<List<AdminRevenueDto>>.Ok(revenue.ToList()));
    }

    [HttpGet("system-stats")]
    public async Task<ActionResult<ApiResponse<AdminSystemStatsDto>>> GetSystemStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(ApiResponse<AdminSystemStatsDto>.Ok(stats));
    }

    [HttpPost("users")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateSubUser(
        [FromBody] CreateSubUserDto dto)
    {
        try
        {
            var currentUserId = GetUserId();
            var user = await _adminService.CreateSubUserAsync(dto, currentUserId);
            return Ok(ApiResponse<AdminUserDto>.Ok(user, "User created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AdminUserDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AdminUserDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AdminUserDto>.Fail(ex.Message));
        }
    }

    [HttpGet("users/activity-summary")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserActivitySummaryDto>>>> GetUsersActivitySummary()
    {
        var summary = await _adminService.GetUsersActivitySummaryAsync();
        return Ok(ApiResponse<IEnumerable<UserActivitySummaryDto>>.Ok(summary));
    }

    [HttpGet("users/{userId:guid}/activity")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserActivityDto>>>> GetUserActivity(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var activity = await _adminService.GetUserActivityAsync(userId, page, pageSize);
        return Ok(ApiResponse<IEnumerable<UserActivityDto>>.Ok(activity));
    }
}
