using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.SuperAdmin;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/super-admin")]
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<SuperAdminAuthResponse>> Login([FromBody] SuperAdminLoginDto dto)
    {
        var result = await _superAdminService.LoginAsync(dto.UserId, dto.Password);
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SuperAdminDashboardDto>>> GetDashboard()
    {
        var dashboard = await _superAdminService.GetDashboardAsync();
        return Ok(ApiResponse<SuperAdminDashboardDto>.Ok(dashboard));
    }

    [HttpGet("chambers")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChamberManageDto>>>> GetAllChambers()
    {
        var chambers = await _superAdminService.GetAllChambersAsync();
        return Ok(ApiResponse<IEnumerable<ChamberManageDto>>.Ok(chambers));
    }

    [HttpGet("chambers/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ChamberManageDto>>> GetChamberDetails(Guid id)
    {
        try
        {
            var chamber = await _superAdminService.GetChamberDetailsAsync(id);
            return Ok(ApiResponse<ChamberManageDto>.Ok(chamber));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ChamberManageDto>.Fail(ex.Message));
        }
    }

    [HttpPut("chambers/{id}/plan")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateChamberPlan(Guid id, [FromBody] UpdateChamberPlanDto dto)
    {
        var (success, message) = await _superAdminService.UpdateChamberPlanAsync(id, dto.Plan);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("chambers/{id}/impersonate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> ImpersonateChamber(
        Guid id, [FromBody] ImpersonateDto? dto)
    {
        var (success, message, token) = await _superAdminService.ImpersonateChamberAsync(
            id, dto?.UserId);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(new { impersonationToken = token, message }));
    }

    [HttpDelete("chambers/{id}/clear")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ClearChamberResult>>> ClearChamber(Guid id)
    {
        var result = await _superAdminService.ClearChamberAsync(id);
        if (!result.Success)
            return BadRequest(ApiResponse<ClearChamberResult>.Fail(result.Message));

        return Ok(ApiResponse<ClearChamberResult>.Ok(result));
    }

    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SuperAdminUserDto>>>> GetAllUsers(
        [FromQuery] Guid? chamberId = null)
    {
        var users = await _superAdminService.GetAllUsersAsync(chamberId);
        return Ok(ApiResponse<IEnumerable<SuperAdminUserDto>>.Ok(users));
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> ResetUserPassword(
        Guid id, [FromBody] ResetPasswordDto dto)
    {
        var (success, message) = await _superAdminService.ResetUserPasswordAsync(id, dto.NewPassword);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPost("users/{id}/toggle-status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid id)
    {
        var (success, message) = await _superAdminService.ToggleUserStatusAsync(id);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("health")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemHealthDto>>> GetSystemHealth()
    {
        var health = await _superAdminService.GetSystemHealthAsync();
        return Ok(ApiResponse<SystemHealthDto>.Ok(health));
    }
}
