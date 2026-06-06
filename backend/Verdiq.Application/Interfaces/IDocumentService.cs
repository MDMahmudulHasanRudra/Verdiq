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

    Task<DocumentResponseDto> UpdateAsync(Guid id, UpdateDocumentDto dto, Guid userId);
    Task ToggleFavoriteAsync(Guid documentId, Guid userId);
    Task<DocumentShareDto> ShareAsync(Guid documentId, ShareDocumentDto dto, Guid sharedById);
    Task RemoveShareAsync(Guid shareId);
    Task<DocumentCommentDto> AddCommentAsync(Guid documentId, AddDocumentCommentDto dto, Guid userId);
    Task<IEnumerable<DocumentCommentDto>> GetCommentsAsync(Guid documentId);
    Task<IEnumerable<DocumentActivityDto>> GetActivityAsync(Guid documentId);
    Task<IEnumerable<DocumentTemplateDto>> GetTemplatesAsync(Guid chamberId, string? category = null);
    Task<DocumentResponseDto> CreateFromTemplateAsync(Guid templateId, Guid caseId, Guid userId);
    Task<DocumentTemplateDto> CreateTemplateAsync(CreateDocumentTemplateDto dto, Guid chamberId, Guid userId, Stream? fileStream = null, string? fileName = null, string? contentType = null);
    Task<IEnumerable<DocumentResponseDto>> SearchAsync(Guid chamberId, string query, int page = 1, int pageSize = 20);
    Task<IEnumerable<DocumentResponseDto>> GetRecentAsync(Guid chamberId, int count = 10);
    Task<IEnumerable<DocumentResponseDto>> GetFavoritesAsync(Guid userId);
    Task RecordViewAsync(Guid documentId, Guid userId);
    Task RecordDownloadAsync(Guid documentId);
    Task<(bool Success, string Message)> BulkDeleteAsync(List<Guid> ids);
    Task<(bool Success, string Message)> BulkUpdateStatusAsync(List<Guid> ids, string status);
    Task<(bool Success, string Message)> BulkUpdateCategoryAsync(List<Guid> ids, string category);
    Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadTemplateAsync(Guid templateId);
}
