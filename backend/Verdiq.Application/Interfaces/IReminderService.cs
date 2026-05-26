using Verdiq.Application.DTOs.Reminder;

namespace Verdiq.Application.Interfaces;

public interface IReminderService
{
    Task<(bool Success, string Message, ReminderResponseDto? Data)> CreateAsync(CreateReminderDto dto, Guid userId);
    Task<IEnumerable<ReminderResponseDto>> GetMyRemindersAsync(Guid userId);
    Task<IEnumerable<ReminderResponseDto>> GetPendingAsync();
    Task MarkAsSentAsync(Guid id);
}
