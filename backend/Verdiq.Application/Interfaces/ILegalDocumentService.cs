using Verdiq.Application.DTOs.LegalDocument;

namespace Verdiq.Application.Interfaces;

public interface ILegalDocumentService
{
    Task<(bool Success, string Message, LegalDocumentResponseDto? Data)> CreateAsync(CreateLegalDocumentDto dto);
    Task<(bool Success, string Message, LegalDocumentResponseDto? Data)> UpdateAsync(Guid id, UpdateLegalDocumentDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<LegalDocumentResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<LegalDocumentResponseDto>> SearchAsync(string query);
    Task<IEnumerable<LegalDocumentResponseDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<LegalDocumentResponseDto>> GetAllAsync(int page = 1, int pageSize = 10);
}
