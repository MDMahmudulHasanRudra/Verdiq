using Verdiq.Application.DTOs.Hearing;

namespace Verdiq.Application.Interfaces;

public interface IHearingService
{
    Task<HearingResponseDto> GetHearingByIdAsync(Guid id);
    Task<IEnumerable<HearingResponseDto>> GetHearingsByCaseIdAsync(Guid caseId);
    Task<IEnumerable<HearingResponseDto>> GetUpcomingHearingsAsync(Guid lawyerId);
    Task<IEnumerable<HearingResponseDto>> GetHearingsByDateAsync(DateTime date, Guid lawyerId);
    Task<HearingResponseDto> CreateHearingAsync(CreateHearingDto dto);
    Task<HearingResponseDto> UpdateHearingAsync(Guid id, UpdateHearingDto dto);
    Task DeleteHearingAsync(Guid id);
    Task SendReminderAsync(Guid hearingId);
}
