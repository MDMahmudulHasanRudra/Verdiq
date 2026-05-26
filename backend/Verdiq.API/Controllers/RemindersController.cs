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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> Create([FromBody] CreateReminderDto dto)
    {
        var userId = GetUserId();
        var (success, message, data) = await _reminderService.CreateAsync(dto, userId);

        if (!success)
            return BadRequest(ApiResponse<ReminderResponseDto>.Fail(message));

        return CreatedAtAction(null, ApiResponse<ReminderResponseDto>.Created(data!));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReminderResponseDto>>>> GetMyReminders()
    {
        var userId = GetUserId();
        var reminders = await _reminderService.GetMyRemindersAsync(userId);
        return Ok(ApiResponse<IEnumerable<ReminderResponseDto>>.Ok(reminders));
    }
}
