using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Admin;
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

    [HttpPost("chambers")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> CreateChamber([FromBody] CreateChamberDto dto)
    {
        var (success, message) = await _superAdminService.CreateChamberAsync(dto);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPut("chambers/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateChamber(Guid id, [FromBody] UpdateChamberDto dto)
    {
        var (success, message) = await _superAdminService.UpdateChamberAsync(id, dto);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
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

    [HttpDelete("chambers/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteChamber(Guid id)
    {
        var (success, message) = await _superAdminService.DeleteChamberAsync(id);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SuperAdminUserDto>>>> GetAllUsers(
        [FromQuery] Guid? chamberId = null, [FromQuery] bool detailed = false)
    {
        var users = detailed
            ? await _superAdminService.GetAllUsersDetailedAsync(chamberId)
            : await _superAdminService.GetAllUsersAsync(chamberId);
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

    [HttpPost("users")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> CreateAdminUser([FromBody] CreateAdminUserDto dto)
    {
        var (success, message) = await _superAdminService.CreateAdminUserAsync(dto);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpPut("users/{id}/subscription")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUserSubscription(
        Guid id, [FromBody] UpdateUserSubscriptionDto dto)
    {
        var (success, message) = await _superAdminService.UpdateUserSubscriptionAsync(id, dto);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("subscriptions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionManageDto>>>> GetAllSubscriptions()
    {
        var subscriptions = await _superAdminService.GetAllSubscriptionsAsync();
        return Ok(ApiResponse<IEnumerable<SubscriptionManageDto>>.Ok(subscriptions));
    }

    [HttpGet("subscriptions/{chamberId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SubscriptionManageDto>>> GetChamberSubscription(Guid chamberId)
    {
        var subscription = await _superAdminService.GetChamberSubscriptionAsync(chamberId);
        if (subscription == null)
            return NotFound(ApiResponse<SubscriptionManageDto>.Fail("Subscription not found"));

        return Ok(ApiResponse<SubscriptionManageDto>.Ok(subscription));
    }

    [HttpGet("permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetAllPermissions()
    {
        var permissions = await _superAdminService.GetAllPermissionsAsync();
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions));
    }

    [HttpGet("role-permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RolePermissionsDto>>>> GetRolePermissions()
    {
        var rolePermissions = await _superAdminService.GetRolePermissionsAsync();
        return Ok(ApiResponse<IEnumerable<RolePermissionsDto>>.Ok(rolePermissions));
    }

    [HttpPut("role-permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> AssignPermissionsToRole(
        [FromBody] RolePermissionAssignmentDto dto)
    {
        var (success, message) = await _superAdminService.AssignPermissionsToRoleAsync(dto.Role, dto.PermissionIds);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpDelete("role-permissions/{role}/{permissionId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> RemovePermissionFromRole(
        string role, Guid permissionId)
    {
        var (success, message) = await _superAdminService.RemovePermissionFromRoleAsync(role, permissionId);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("audit-logs")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _superAdminService.GetAuditLogsAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<AuditLogDto>>.Ok(logs));
    }

    [HttpGet("billing")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<BillingOverviewDto>>> GetBillingOverview()
    {
        var billing = await _superAdminService.GetBillingOverviewAsync();
        return Ok(ApiResponse<BillingOverviewDto>.Ok(billing));
    }

    [HttpPost("broadcast")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> BroadcastNotification(
        [FromBody] BroadcastNotificationDto dto)
    {
        var (success, message) = await _superAdminService.BroadcastNotificationAsync(dto);
        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        return Ok(ApiResponse<object>.Ok(null!, message));
    }

    [HttpGet("config")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemConfigDto>>> GetSystemConfig()
    {
        var config = await _superAdminService.GetSystemConfigAsync();
        return Ok(ApiResponse<SystemConfigDto>.Ok(config));
    }

    [HttpPut("config")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSystemConfig([FromBody] SystemConfigDto dto)
    {
        var (success, message) = await _superAdminService.UpdateSystemConfigAsync(dto);
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

    [HttpGet("cases")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminCaseDto>>>> GetAllCases()
    {
        var cases = await _superAdminService.GetAllCasesAsync();
        return Ok(ApiResponse<IEnumerable<AdminCaseDto>>.Ok(cases));
    }

    [HttpGet("revenue-chart")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetRevenueChart(
        [FromQuery] int months = 12)
    {
        var data = await _superAdminService.GetRevenueChartDataAsync(months);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(data));
    }

    [HttpGet("chamber-growth")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetChamberGrowth(
        [FromQuery] int months = 12)
    {
        var data = await _superAdminService.GetChamberGrowthDataAsync(months);
        return Ok(ApiResponse<IEnumerable<object>>.Ok(data));
    }
}
