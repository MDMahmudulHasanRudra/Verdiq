using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Organization;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : BaseController
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> Create([FromBody] CreateOrganizationDto dto)
    {
        var userId = GetUserId();
        var org = await _organizationService.CreateOrganizationAsync(dto, userId);
        return Ok(ApiResponse<OrganizationDetailDto>.Created(org));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrganizationDto>>>> GetMyOrganizations()
    {
        var userId = GetUserId();
        var orgs = await _organizationService.GetUserOrganizationsAsync(userId);
        return Ok(ApiResponse<List<OrganizationDto>>.Ok(orgs));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> GetById(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var org = await _organizationService.GetOrganizationAsync(id, userId);
            return Ok(ApiResponse<OrganizationDetailDto>.Ok(org));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrganizationDetailDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDetailDto>>> Update(Guid id, [FromBody] UpdateOrganizationDto dto)
    {
        try
        {
            var userId = GetUserId();
            var org = await _organizationService.UpdateOrganizationAsync(id, dto, userId);
            return Ok(ApiResponse<OrganizationDetailDto>.Ok(org));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrganizationDetailDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _organizationService.DeleteOrganizationAsync(id, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Organization deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<ApiResponse<List<OrganizationMemberDto>>>> GetMembers(Guid id)
    {
        var members = await _organizationService.GetMembersAsync(id);
        return Ok(ApiResponse<List<OrganizationMemberDto>>.Ok(members));
    }

    [HttpPost("{id}/members")]
    public async Task<ActionResult<ApiResponse<OrganizationMemberDto>>> InviteMember(Guid id, [FromBody] InviteMemberDto dto)
    {
        try
        {
            var userId = GetUserId();
            var member = await _organizationService.InviteMemberAsync(id, dto, userId);
            return Ok(ApiResponse<OrganizationMemberDto>.Ok(member));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrganizationMemberDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}/members/{memberId}/role")]
    public async Task<ActionResult<ApiResponse<OrganizationMemberDto>>> UpdateMemberRole(
        Guid id, Guid memberId, [FromBody] UpdateMemberRoleDto dto)
    {
        try
        {
            var userId = GetUserId();
            var member = await _organizationService.UpdateMemberRoleAsync(id, memberId, dto, userId);
            return Ok(ApiResponse<OrganizationMemberDto>.Ok(member));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrganizationMemberDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveMember(Guid id, Guid memberId)
    {
        try
        {
            var userId = GetUserId();
            await _organizationService.RemoveMemberAsync(id, memberId, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Member removed"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/workspaces")]
    public async Task<ActionResult<ApiResponse<List<WorkspaceDto>>>> GetWorkspaces(Guid id)
    {
        var workspaces = await _organizationService.GetWorkspacesAsync(id);
        return Ok(ApiResponse<List<WorkspaceDto>>.Ok(workspaces));
    }

    [HttpPost("{id}/workspaces")]
    public async Task<ActionResult<ApiResponse<WorkspaceDto>>> CreateWorkspace(Guid id, [FromBody] CreateWorkspaceDto dto)
    {
        try
        {
            var userId = GetUserId();
            var workspace = await _organizationService.CreateWorkspaceAsync(id, dto, userId);
            return Ok(ApiResponse<WorkspaceDto>.Created(workspace));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("workspaces/{workspaceId}")]
    public async Task<ActionResult<ApiResponse<WorkspaceDto>>> UpdateWorkspace(Guid workspaceId, [FromBody] CreateWorkspaceDto dto)
    {
        try
        {
            var userId = GetUserId();
            var workspace = await _organizationService.UpdateWorkspaceAsync(workspaceId, dto, userId);
            return Ok(ApiResponse<WorkspaceDto>.Ok(workspace));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<WorkspaceDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("workspaces/{workspaceId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWorkspace(Guid workspaceId)
    {
        try
        {
            var userId = GetUserId();
            await _organizationService.DeleteWorkspaceAsync(workspaceId, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Workspace deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
