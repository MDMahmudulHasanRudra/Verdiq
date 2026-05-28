using Verdiq.Application.DTOs.LegalSection;

namespace Verdiq.Application.Interfaces;

public interface ILegalSectionService
{
    Task<(bool Success, string Message, LegalSectionResponseDto? Data)> CreateAsync(CreateLegalSectionDto dto, Guid chamberId);
    Task<(bool Success, string Message, LegalSectionResponseDto? Data)> UpdateAsync(Guid id, UpdateLegalSectionDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<LegalSectionResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LegalSectionResponseDto>> GetAllAsync(Guid chamberId, string? category = null, string? search = null);
    Task<IEnumerable<LegalSectionResponseDto>> SearchAsync(string query, Guid chamberId);
}
