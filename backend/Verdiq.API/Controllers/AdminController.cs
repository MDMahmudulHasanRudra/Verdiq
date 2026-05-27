using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Admin;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
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
}
