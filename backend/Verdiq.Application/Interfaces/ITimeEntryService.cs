using Verdiq.Application.DTOs.TimeEntry;

namespace Verdiq.Application.Interfaces;

public interface ITimeEntryService
{
    Task<IEnumerable<TimeEntryResponseDto>> GetAllAsync(Guid chamberId, string? status = null, DateTime? from = null, DateTime? to = null);
    Task<TimeEntryResponseDto?> GetByIdAsync(Guid id, Guid chamberId);
    Task<TimeEntryResponseDto> CreateAsync(CreateTimeEntryDto dto, Guid chamberId, Guid userId);
    Task<TimeEntryResponseDto?> UpdateAsync(Guid id, UpdateTimeEntryDto dto, Guid chamberId);
    Task<TimeEntryResponseDto?> UpdateStatusAsync(Guid id, UpdateTimeEntryStatusDto dto, Guid chamberId);
    Task<TimeEntryResponseDto?> StopTimerAsync(Guid id, Guid chamberId);
    Task<bool> DeleteAsync(Guid id, Guid chamberId);
    Task<TimeEntryResponseDto?> GetRunningTimerAsync(Guid userId);
    Task<TimeSheetAnalyticsDto> GetAnalyticsAsync(Guid chamberId, DateTime? from = null, DateTime? to = null);
    Task<TeamCapacityDto> GetTeamCapacityAsync(Guid chamberId);
    Task<TimeEntryResponseDto?> ApproveAsync(Guid id, Guid chamberId);
    Task<TimeEntryResponseDto?> RejectAsync(Guid id, Guid chamberId);
    Task<IEnumerable<TimeEntryResponseDto>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<TimeEntryResponseDto>> GetByCaseAsync(Guid caseId, Guid chamberId);
    Task<IEnumerable<TimeEntryResponseDto>> GetByInvoiceAsync(Guid invoiceId, Guid chamberId);
    Task<List<TimeEntryResponseDto>> GetUninvoicedAsync(Guid chamberId, Guid? clientId = null);
    Task<bool> MarkAsInvoicedAsync(List<Guid> entryIds, Guid invoiceId, Guid chamberId);
}
