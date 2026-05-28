using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Reminder;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController : BaseController
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReminderResponseDto>>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? type, [FromQuery] string? priority)
    {
        var chamberId = GetChamberId();
        var reminders = await _reminderService.GetAllAsync(chamberId, status, type, priority);
        return Ok(ApiResponse<IEnumerable<ReminderResponseDto>>.Ok(reminders));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReminderResponseDto>>>> GetMyReminders([FromQuery] string? status)
    {
        var userId = GetUserId();
        var reminders = await _reminderService.GetMyRemindersAsync(userId, status);
        return Ok(ApiResponse<IEnumerable<ReminderResponseDto>>.Ok(reminders));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> GetById(Guid id)
    {
        var chamberId = GetChamberId();
        var reminder = await _reminderService.GetByIdAsync(id, chamberId);
        if (reminder == null) return NotFound(ApiResponse<ReminderResponseDto>.Fail("Reminder not found"));
        return Ok(ApiResponse<ReminderResponseDto>.Ok(reminder));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> Create([FromBody] CreateReminderDto dto)
    {
        var chamberId = GetChamberId();
        var reminder = await _reminderService.CreateAsync(dto, chamberId);
        return CreatedAtAction(nameof(GetById), new { id = reminder.Id }, ApiResponse<ReminderResponseDto>.Created(reminder));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> UpdateStatus(Guid id, [FromBody] UpdateReminderStatusDto dto)
    {
        var chamberId = GetChamberId();
        var reminder = await _reminderService.UpdateStatusAsync(id, dto, chamberId);
        if (reminder == null) return NotFound(ApiResponse<ReminderResponseDto>.Fail("Reminder not found"));
        return Ok(ApiResponse<ReminderResponseDto>.Ok(reminder));
    }

    [HttpPost("{id:guid}/snooze")]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> Snooze(Guid id, [FromBody] SnoozeReminderDto dto)
    {
        var chamberId = GetChamberId();
        var reminder = await _reminderService.SnoozeAsync(id, dto, chamberId);
        if (reminder == null) return NotFound(ApiResponse<ReminderResponseDto>.Fail("Reminder not found"));
        return Ok(ApiResponse<ReminderResponseDto>.Ok(reminder));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var chamberId = GetChamberId();
        var deleted = await _reminderService.DeleteAsync(id, chamberId);
        if (!deleted) return NotFound(ApiResponse<string>.Fail("Reminder not found"));
        return Ok(ApiResponse<string>.Ok("Reminder deleted"));
    }

    [HttpPost("bulk-mark-read")]
    public async Task<ActionResult<ApiResponse<string>>> BulkMarkRead([FromBody] BulkReminderActionDto dto)
    {
        var chamberId = GetChamberId();
        await _reminderService.BulkMarkReadAsync(dto.Ids, chamberId);
        return Ok(ApiResponse<string>.Ok("Marked as read"));
    }

    [HttpPost("bulk-complete")]
    public async Task<ActionResult<ApiResponse<string>>> BulkComplete([FromBody] BulkReminderActionDto dto)
    {
        var chamberId = GetChamberId();
        await _reminderService.BulkCompleteAsync(dto.Ids, chamberId);
        return Ok(ApiResponse<string>.Ok("Completed"));
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<ApiResponse<string>>> BulkDelete([FromBody] BulkReminderActionDto dto)
    {
        var chamberId = GetChamberId();
        await _reminderService.BulkDeleteAsync(dto.Ids, chamberId);
        return Ok(ApiResponse<string>.Ok("Deleted"));
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<ReminderAnalyticsDto>>> GetAnalytics()
    {
        var chamberId = GetChamberId();
        var analytics = await _reminderService.GetAnalyticsAsync(chamberId);
        return Ok(ApiResponse<ReminderAnalyticsDto>.Ok(analytics));
    }

    [HttpGet("agenda")]
    public async Task<ActionResult<ApiResponse<DailyAgendaDto>>> GetDailyAgenda()
    {
        var userId = GetUserId();
        var chamberId = GetChamberId();
        var agenda = await _reminderService.GetDailyAgendaAsync(userId, chamberId);
        return Ok(ApiResponse<DailyAgendaDto>.Ok(agenda));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _reminderService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpGet("next-upcoming")]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto?>>> GetNextUpcoming()
    {
        var userId = GetUserId();
        var reminder = await _reminderService.GetNextUpcomingAsync(userId);
        return Ok(ApiResponse<ReminderResponseDto?>.Ok(reminder));
    }

    [HttpPost("evaluate")]
    public async Task<ActionResult<ApiResponse<string>>> EvaluateRules()
    {
        var chamberId = GetChamberId();
        await _reminderService.EvaluateAutomationRulesAsync(chamberId);
        return Ok(ApiResponse<string>.Ok("Rules evaluated"));
    }
}
