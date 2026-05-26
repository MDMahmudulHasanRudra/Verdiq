using Verdiq.Application.DTOs.Document;

namespace Verdiq.Application.Interfaces;

public interface IDocumentService
{
    Task<(bool Success, string Message, DocumentResponseDto? Data)> UploadAsync(
        Guid caseId, Guid userId, string category, string? folderPath, Stream fileStream, string fileName, string contentType);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<DocumentResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<DocumentResponseDto>> GetByCaseIdAsync(Guid caseId);
    Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAsync(Guid id);
    Task<IEnumerable<DocumentResponseDto>> GetAllAsync(Guid chamberId, string? category = null, int page = 1, int pageSize = 10);
}
