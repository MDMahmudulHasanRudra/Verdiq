using Verdiq.Application.DTOs.Document;

namespace Verdiq.Application.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync(Guid? caseId = null, string? category = null, string? tag = null);
    Task<DocumentResponseDto> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentResponseDto>> GetDocumentsByCaseIdAsync(Guid caseId);
    Task<DocumentResponseDto> UploadDocumentAsync(Stream fileStream, string fileName, string contentType,
        long fileSize, string documentType, string category, Guid caseId, Guid uploadedById, List<string>? tags = null);
    Task DeleteDocumentAsync(Guid id);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(Guid id);

    Task<DocumentResponseDto> UploadNewVersionAsync(Guid documentId, Stream fileStream, string fileName,
        string contentType, long fileSize, Guid uploadedById, string? changeNotes = null);
    Task<List<DocumentVersionDto>> GetVersionHistoryAsync(Guid documentId);
    Task<DocumentResponseDto> RestoreVersionAsync(Guid documentId, Guid versionId, Guid userId);
    Task<string> GenerateSignedUrlAsync(Guid documentId);

    Task AddTagAsync(Guid documentId, string tagName);
    Task RemoveTagAsync(Guid documentId, string tagName);
    Task<List<DocumentTagDto>> GetTagsAsync(Guid documentId);

    Task<BulkOperationResult> BulkDeleteAsync(List<Guid> ids);
}
