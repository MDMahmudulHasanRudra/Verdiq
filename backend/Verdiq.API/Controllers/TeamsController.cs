using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Team;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : BaseController
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamResponseDto>>>> GetAll()
    {
        var chamberId = GetChamberId();
        var teams = await _teamService.GetAllAsync(chamberId);
        return Ok(ApiResponse<List<TeamResponseDto>>.Ok(teams.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TeamResponseDto>>> GetById(Guid id)
    {
        var team = await _teamService.GetByIdAsync(id);
        if (team is null)
            return NotFound(ApiResponse<TeamResponseDto>.Fail("Team not found"));
        return Ok(ApiResponse<TeamResponseDto>.Ok(team));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TeamResponseDto>>> Create([FromBody] CreateTeamDto dto)
    {
        var userId = GetUserId();
        var chamberId = GetChamberId();
        var team = await _teamService.CreateAsync(dto, userId, chamberId);
        return CreatedAtAction(nameof(GetById), new { id = team.Id },
            ApiResponse<TeamResponseDto>.Created(team));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TeamResponseDto>>> Update(Guid id, [FromBody] UpdateTeamDto dto)
    {
        try
        {
            var team = await _teamService.UpdateAsync(id, dto);
            return Ok(ApiResponse<TeamResponseDto>.Ok(team));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TeamResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _teamService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Team deleted"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/members")]
    public async Task<ActionResult<ApiResponse<TeamMemberDto>>> AddMember(Guid id, [FromBody] AddTeamMemberDto dto)
    {
        try
        {
            var member = await _teamService.AddMemberAsync(id, dto);
            return Ok(ApiResponse<TeamMemberDto>.Ok(member));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TeamMemberDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<TeamMemberDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveMember(Guid id, Guid userId)
    {
        try
        {
            await _teamService.RemoveMemberAsync(id, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Member removed"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/members/{userId}/role")]
    public async Task<ActionResult<ApiResponse<TeamMemberDto>>> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateTeamMemberRoleDto dto)
    {
        try
        {
            var member = await _teamService.UpdateMemberRoleAsync(id, userId, dto);
            return Ok(ApiResponse<TeamMemberDto>.Ok(member));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TeamMemberDto>.Fail(ex.Message));
        }
    }
}
