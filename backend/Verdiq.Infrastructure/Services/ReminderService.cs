using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Reminder;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _context;

    public ReminderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, ReminderResponseDto? Data)> CreateAsync(CreateReminderDto dto, Guid userId)
    {
        var reminder = new Reminder
        {
            UserId = userId,
            Type = dto.Type,
            Channel = Enum.TryParse<ReminderChannel>(dto.Channel, true, out var channel) ? channel : ReminderChannel.PushNotification,
            ScheduledAt = DateTime.SpecifyKind(dto.ScheduledAt, DateTimeKind.Utc),
            Message = dto.Message,
            ReferenceId = dto.ReferenceId,
            SentStatus = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var result = MapToDto(reminder);
        return (true, "Reminder created successfully", result);
    }

    public async Task<IEnumerable<ReminderResponseDto>> GetMyRemindersAsync(Guid userId)
    {
        var reminders = await _context.Reminders
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reminders.Select(MapToDto);
    }

    public async Task<IEnumerable<ReminderResponseDto>> GetPendingAsync()
    {
        var now = DateTime.UtcNow;
        var reminders = await _context.Reminders
            .Where(r => !r.SentStatus && r.ScheduledAt <= now)
            .OrderBy(r => r.ScheduledAt)
            .ToListAsync();

        return reminders.Select(MapToDto);
    }

    public async Task MarkAsSentAsync(Guid id)
    {
        var reminder = await _context.Reminders.FindAsync(id);
        if (reminder == null) return;

        reminder.SentStatus = true;
        reminder.SentAt = DateTime.UtcNow;
        reminder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static ReminderResponseDto MapToDto(Reminder r)
    {
        return new ReminderResponseDto
        {
            Id = r.Id,
            Type = r.Type,
            Channel = r.Channel.ToString(),
            ScheduledAt = r.ScheduledAt,
            Message = r.Message,
            SentStatus = r.SentStatus,
            SentAt = r.SentAt,
            CreatedAt = r.CreatedAt
        };
    }
}
