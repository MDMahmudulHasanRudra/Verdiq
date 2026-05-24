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
        var users = await _adminService.GetUsersAsync(search);
        return Ok(ApiResponse<List<AdminUserDto>>.Ok(users));
    }

    [HttpPatch("users/{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateUserStatus(
        Guid id, [FromBody] UpdateUserStatusDto dto)
    {
        try
        {
            var user = await _adminService.UpdateUserStatusAsync(id, dto.IsActive);
            return Ok(ApiResponse<AdminUserDto>.Ok(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AdminUserDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
    {
        try
        {
            await _adminService.DeleteUserAsync(id);
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
        return Ok(ApiResponse<List<AdminCaseDto>>.Ok(cases));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<List<AdminRevenueDto>>>> GetRevenue(
        [FromQuery] int months = 6)
    {
        var revenue = await _adminService.GetRevenueAsync(months);
        return Ok(ApiResponse<List<AdminRevenueDto>>.Ok(revenue));
    }

    [HttpGet("system-stats")]
    public async Task<ActionResult<ApiResponse<AdminSystemStatsDto>>> GetSystemStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(ApiResponse<AdminSystemStatsDto>.Ok(stats));
    }
}
