using Verdiq.Application.DTOs.LegalDocument;

namespace Verdiq.Application.Interfaces;

public interface ILegalDocumentService
{
    Task<(bool Success, string Message, LegalDocumentResponseDto? Data)> CreateAsync(CreateLegalDocumentDto dto);
    Task<IEnumerable<LegalDocumentResponseDto>> SearchAsync(string query);
    Task<IEnumerable<LegalDocumentResponseDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<LegalDocumentResponseDto>> GetAllAsync(int page = 1, int pageSize = 10);
}
