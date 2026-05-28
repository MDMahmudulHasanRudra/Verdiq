using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.TimeEntry;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/time-entries")]
[Authorize]
public class TimeEntriesController : BaseController
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntriesController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimeEntryResponseDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var chamberId = GetChamberId();
        var entries = await _timeEntryService.GetAllAsync(chamberId, status, from, to);
        return Ok(ApiResponse<IEnumerable<TimeEntryResponseDto>>.Ok(entries));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> GetById(Guid id)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.GetByIdAsync(id, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> Create([FromBody] CreateTimeEntryDto dto)
    {
        var chamberId = GetChamberId();
        var userId = GetUserId();
        var entry = await _timeEntryService.CreateAsync(dto, chamberId, userId);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, ApiResponse<TimeEntryResponseDto>.Created(entry));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> Update(Guid id, [FromBody] UpdateTimeEntryDto dto)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.UpdateAsync(id, dto, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> UpdateStatus(Guid id, [FromBody] UpdateTimeEntryStatusDto dto)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.UpdateStatusAsync(id, dto, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }

    [HttpPost("{id:guid}/stop")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> StopTimer(Guid id)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.StopTimerAsync(id, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var chamberId = GetChamberId();
        var deleted = await _timeEntryService.DeleteAsync(id, chamberId);
        if (!deleted) return NotFound(ApiResponse<string>.Fail("Time entry not found"));
        return Ok(ApiResponse<string>.Ok("Time entry deleted"));
    }

    [HttpGet("running")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto?>>> GetRunningTimer()
    {
        var userId = GetUserId();
        var entry = await _timeEntryService.GetRunningTimerAsync(userId);
        return Ok(ApiResponse<TimeEntryResponseDto?>.Ok(entry));
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<TimeSheetAnalyticsDto>>> GetAnalytics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var chamberId = GetChamberId();
        var analytics = await _timeEntryService.GetAnalyticsAsync(chamberId, from, to);
        return Ok(ApiResponse<TimeSheetAnalyticsDto>.Ok(analytics));
    }

    [HttpGet("team-capacity")]
    public async Task<ActionResult<ApiResponse<TeamCapacityDto>>> GetTeamCapacity()
    {
        var chamberId = GetChamberId();
        var capacity = await _timeEntryService.GetTeamCapacityAsync(chamberId);
        return Ok(ApiResponse<TeamCapacityDto>.Ok(capacity));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimeEntryResponseDto>>>> GetByUser(
        Guid userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var entries = await _timeEntryService.GetByUserAsync(userId, from, to);
        return Ok(ApiResponse<IEnumerable<TimeEntryResponseDto>>.Ok(entries));
    }

    [HttpGet("by-case/{caseId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimeEntryResponseDto>>>> GetByCase(Guid caseId)
    {
        var chamberId = GetChamberId();
        var entries = await _timeEntryService.GetByCaseAsync(caseId, chamberId);
        return Ok(ApiResponse<IEnumerable<TimeEntryResponseDto>>.Ok(entries));
    }

    [HttpGet("uninvoiced")]
    public async Task<ActionResult<ApiResponse<List<TimeEntryResponseDto>>>> GetUninvoiced([FromQuery] Guid? clientId)
    {
        var chamberId = GetChamberId();
        var entries = await _timeEntryService.GetUninvoicedAsync(chamberId, clientId);
        return Ok(ApiResponse<List<TimeEntryResponseDto>>.Ok(entries));
    }

    [HttpPost("mark-invoiced")]
    public async Task<ActionResult<ApiResponse<string>>> MarkAsInvoiced([FromBody] MarkInvoicedDto dto)
    {
        var chamberId = GetChamberId();
        var result = await _timeEntryService.MarkAsInvoicedAsync(dto.EntryIds, dto.InvoiceId, chamberId);
        if (!result) return BadRequest(ApiResponse<string>.Fail("No entries were updated"));
        return Ok(ApiResponse<string>.Ok("Entries marked as invoiced"));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> Approve(Guid id)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.ApproveAsync(id, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<TimeEntryResponseDto>>> Reject(Guid id)
    {
        var chamberId = GetChamberId();
        var entry = await _timeEntryService.RejectAsync(id, chamberId);
        if (entry == null) return NotFound(ApiResponse<TimeEntryResponseDto>.Fail("Time entry not found"));
        return Ok(ApiResponse<TimeEntryResponseDto>.Ok(entry));
    }
}

public class MarkInvoicedDto
{
    public List<Guid> EntryIds { get; set; } = new();
    public Guid InvoiceId { get; set; }
}
