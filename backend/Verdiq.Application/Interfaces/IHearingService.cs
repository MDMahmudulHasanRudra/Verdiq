using Verdiq.Application.DTOs.Hearing;

namespace Verdiq.Application.Interfaces;

public interface IHearingService
{
    Task<(bool Success, string Message, HearingResponseDto? Data)> CreateAsync(CreateHearingDto dto, Guid chamberId);
    Task<(bool Success, string Message, HearingResponseDto? Data)> UpdateAsync(Guid id, UpdateHearingDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<HearingResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<HearingResponseDto>> GetByCaseIdAsync(Guid caseId);
    Task<IEnumerable<HearingResponseDto>> GetUpcomingAsync(Guid chamberId);
    Task<IEnumerable<HearingResponseDto>> GetByDateAsync(DateTime date, Guid chamberId);
}
