using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Hearing;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HearingsController : BaseController
{
    private readonly IHearingService _hearingService;

    public HearingsController(IHearingService hearingService)
    {
        _hearingService = hearingService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<HearingResponseDto>>> GetAll(int page = 1, int pageSize = 10)
    {
        var chamberId = GetChamberId();
        var hearings = await _hearingService.GetUpcomingAsync(chamberId);
        var totalCount = hearings.Count();
        var paged = hearings.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResponse<HearingResponseDto>
        {
            Data = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<List<HearingResponseDto>>>> GetUpcoming()
    {
        var chamberId = GetChamberId();
        var hearings = await _hearingService.GetUpcomingAsync(chamberId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<ApiResponse<List<HearingResponseDto>>>> GetByDate([FromQuery] DateOnly date)
    {
        var chamberId = GetChamberId();
        var hearings = await _hearingService.GetByDateAsync(date.ToDateTime(TimeOnly.MinValue), chamberId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<List<HearingResponseDto>>>> GetByCase(Guid caseId)
    {
        var hearings = await _hearingService.GetByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> GetById(Guid id)
    {
        var hearing = await _hearingService.GetByIdAsync(id);
        if (hearing is null)
            return NotFound(ApiResponse<HearingResponseDto>.Fail("Hearing not found"));
        return Ok(ApiResponse<HearingResponseDto>.Ok(hearing));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> Create([FromBody] CreateHearingDto dto)
    {
        var chamberId = GetChamberId();
        var (success, message, data) = await _hearingService.CreateAsync(dto, chamberId);
        if (!success)
            return BadRequest(ApiResponse<HearingResponseDto>.Fail(message));
        return CreatedAtAction(nameof(GetById), new { id = data!.Id },
            ApiResponse<HearingResponseDto>.Created(data));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> Update(Guid id, [FromBody] UpdateHearingDto dto)
    {
        var (success, message, data) = await _hearingService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(ApiResponse<HearingResponseDto>.Fail(message));
        return Ok(ApiResponse<HearingResponseDto>.Ok(data!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var (success, message) = await _hearingService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(null!, message));
    }
}
