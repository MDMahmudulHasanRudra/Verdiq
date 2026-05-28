using Verdiq.Application.DTOs.Reminder;

namespace Verdiq.Application.Interfaces;

public interface IReminderService
{
    Task<ReminderResponseDto> CreateAsync(CreateReminderDto dto, Guid chamberId);
    Task<ReminderResponseDto?> GetByIdAsync(Guid id, Guid chamberId);
    Task<IEnumerable<ReminderResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? type = null, string? priority = null);
    Task<IEnumerable<ReminderResponseDto>> GetMyRemindersAsync(Guid userId, string? status = null);
    Task<ReminderResponseDto?> UpdateStatusAsync(Guid id, UpdateReminderStatusDto dto, Guid chamberId);
    Task<ReminderResponseDto?> SnoozeAsync(Guid id, SnoozeReminderDto dto, Guid chamberId);
    Task<bool> DeleteAsync(Guid id, Guid chamberId);
    Task<bool> BulkMarkReadAsync(List<Guid> ids, Guid chamberId);
    Task<bool> BulkCompleteAsync(List<Guid> ids, Guid chamberId);
    Task<bool> BulkDeleteAsync(List<Guid> ids, Guid chamberId);
    Task<ReminderAnalyticsDto> GetAnalyticsAsync(Guid chamberId);
    Task<DailyAgendaDto> GetDailyAgendaAsync(Guid userId, Guid chamberId);
    Task<ReminderResponseDto?> GetNextUpcomingAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task EvaluateAutomationRulesAsync(Guid chamberId);
    Task EvaluateAllChambersAsync();
}
