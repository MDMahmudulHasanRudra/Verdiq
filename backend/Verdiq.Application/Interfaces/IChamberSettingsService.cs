using Verdiq.Application.DTOs.ChamberSettings;

namespace Verdiq.Application.Interfaces;

public interface IChamberSettingsService
{
    Task<(bool Success, string Message, ChamberSettingsDto? Data)> GetSettingsAsync(Guid chamberId);
    Task<(bool Success, string Message, ChamberSettingsDto? Data)> UpdateSettingsAsync(Guid chamberId, UpdateChamberSettingsDto dto, Guid userId);
    Task<(bool Success, string Message, object? Data)> GetSubsectionAsync(Guid chamberId, string subsection);
    Task<(bool Success, string Message, ChamberSettingsDto? Data)> UpdateSubsectionAsync(Guid chamberId, string subsection, Dictionary<string, object> values, Guid userId);
}
