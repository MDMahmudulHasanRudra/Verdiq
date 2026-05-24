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
    public async Task<ActionResult<PagedResponse<HearingResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string sortBy = "hearingDate",
        [FromQuery] string sortOrder = "desc")
    {
        var userId = GetUserId();
        var all = await _hearingService.GetUpcomingHearingsAsync(userId);
        var past = await _hearingService.GetHearingsByDateAsync(DateTime.MinValue, userId);
        var hearings = all.Concat(past).DistinctBy(h => h.Id).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            hearings = hearings.Where(h =>
                h.CaseTitle.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                h.CaseNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                h.Court.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(status))
            hearings = hearings.Where(h => h.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(type))
            hearings = hearings.Where(h => h.HearingType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

        if (dateFrom.HasValue)
            hearings = hearings.Where(h => h.HearingDate >= dateFrom.Value).ToList();

        if (dateTo.HasValue)
            hearings = hearings.Where(h => h.HearingDate <= dateTo.Value).ToList();

        hearings = sortBy.ToLower() switch
        {
            "date" => sortOrder == "asc" ? hearings.OrderBy(h => h.HearingDate).ToList() : hearings.OrderByDescending(h => h.HearingDate).ToList(),
            "court" => sortOrder == "asc" ? hearings.OrderBy(h => h.Court).ToList() : hearings.OrderByDescending(h => h.Court).ToList(),
            "type" => sortOrder == "asc" ? hearings.OrderBy(h => h.HearingType).ToList() : hearings.OrderByDescending(h => h.HearingType).ToList(),
            _ => sortOrder == "asc" ? hearings.OrderBy(h => h.HearingDate).ThenBy(h => h.Time).ToList() : hearings.OrderByDescending(h => h.HearingDate).ThenByDescending(h => h.Time).ToList(),
        };

        var totalCount = hearings.Count;
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
        var userId = GetUserId();
        var hearings = await _hearingService.GetUpcomingHearingsAsync(userId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<ApiResponse<List<HearingResponseDto>>>> GetByDate([FromQuery] DateTime date)
    {
        var userId = GetUserId();
        var hearings = await _hearingService.GetHearingsByDateAsync(date, userId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("by-case/{caseId}")]
    public async Task<ActionResult<ApiResponse<List<HearingResponseDto>>>> GetByCase(Guid caseId)
    {
        var hearings = await _hearingService.GetHearingsByCaseIdAsync(caseId);
        return Ok(ApiResponse<List<HearingResponseDto>>.Ok(hearings.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> GetById(Guid id)
    {
        try
        {
            var hearing = await _hearingService.GetHearingByIdAsync(id);
            return Ok(ApiResponse<HearingResponseDto>.Ok(hearing));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HearingResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> Create([FromBody] CreateHearingDto dto)
    {
        try
        {
            var hearing = await _hearingService.CreateHearingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = hearing.Id },
                ApiResponse<HearingResponseDto>.Created(hearing));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ApiResponse<HearingResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<HearingResponseDto>>> Update(Guid id, [FromBody] UpdateHearingDto dto)
    {
        try
        {
            var hearing = await _hearingService.UpdateHearingAsync(id, dto);
            return Ok(ApiResponse<HearingResponseDto>.Ok(hearing));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HearingResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _hearingService.DeleteHearingAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Hearing deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/send-reminder")]
    public async Task<ActionResult<ApiResponse<object>>> SendReminder(Guid id)
    {
        try
        {
            await _hearingService.SendReminderAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Reminder sent successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
