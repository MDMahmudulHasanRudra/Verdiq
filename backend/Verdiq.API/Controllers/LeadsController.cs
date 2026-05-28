using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Lead;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public class LeadsController : BaseController
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeadResponseDto>>>> GetAll()
    {
        var chamberId = GetChamberId();
        var leads = await _leadService.GetAllAsync(chamberId);
        return Ok(ApiResponse<IEnumerable<LeadResponseDto>>.Ok(leads));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> GetById(Guid id)
    {
        var chamberId = GetChamberId();
        var lead = await _leadService.GetByIdAsync(id, chamberId);
        if (lead == null) return NotFound(ApiResponse<LeadResponseDto>.Fail("Lead not found"));
        return Ok(ApiResponse<LeadResponseDto>.Ok(lead));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> Create([FromBody] CreateLeadDto dto)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        var lead = await _leadService.CreateAsync(dto, chamberId, userId);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, ApiResponse<LeadResponseDto>.Created(lead));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> Update(Guid id, [FromBody] UpdateLeadDto dto)
    {
        var chamberId = GetChamberId();
        var lead = await _leadService.UpdateAsync(id, dto, chamberId);
        if (lead == null) return NotFound(ApiResponse<LeadResponseDto>.Fail("Lead not found"));
        return Ok(ApiResponse<LeadResponseDto>.Ok(lead));
    }

    [HttpPatch("{id:guid}/stage")]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> UpdateStage(Guid id, [FromBody] UpdateLeadStageDto dto)
    {
        var chamberId = GetChamberId();
        var lead = await _leadService.UpdateStageAsync(id, dto, chamberId);
        if (lead == null) return NotFound(ApiResponse<LeadResponseDto>.Fail("Lead not found"));
        return Ok(ApiResponse<LeadResponseDto>.Ok(lead));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var chamberId = GetChamberId();
        var deleted = await _leadService.DeleteAsync(id, chamberId);
        if (!deleted) return NotFound(ApiResponse<string>.Fail("Lead not found"));
        return Ok(ApiResponse<string>.Ok("Lead deleted"));
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<LeadAnalyticsDto>>> GetAnalytics()
    {
        var chamberId = GetChamberId();
        var analytics = await _leadService.GetAnalyticsAsync(chamberId);
        return Ok(ApiResponse<LeadAnalyticsDto>.Ok(analytics));
    }

    [HttpGet("by-stage/{stage}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeadResponseDto>>>> GetByStage(string stage)
    {
        var chamberId = GetChamberId();
        var leads = await _leadService.GetByStageAsync(stage, chamberId);
        return Ok(ApiResponse<IEnumerable<LeadResponseDto>>.Ok(leads));
    }
}
