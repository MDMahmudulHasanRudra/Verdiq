using Verdiq.Application.DTOs.Document;

namespace Verdiq.Application.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(Guid? caseId = null, string? category = null);
    Task<DocumentResponseDto> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentResponseDto>> GetDocumentsByCaseIdAsync(Guid caseId);
    Task<DocumentResponseDto> UploadDocumentAsync(Stream fileStream, string fileName, string contentType,
        long fileSize, string documentType, string category, Guid caseId, Guid uploadedById);
    Task DeleteDocumentAsync(Guid id);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(Guid id);
}
