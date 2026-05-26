using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Permission;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RolePermissionResponseDto>>>> GetAllRolePermissions()
    {
        var permissions = await _permissionService.GetAllRolePermissionsAsync();
        return Ok(ApiResponse<IEnumerable<RolePermissionResponseDto>>.Ok(permissions));
    }

    [HttpGet("check")]
    public async Task<ActionResult<ApiResponse<bool>>> HasPermission([FromQuery] string role, [FromQuery] string permission)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(role, permission);
        return Ok(ApiResponse<bool>.Ok(hasPermission));
    }
}
