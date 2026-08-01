using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Interfaces;

public interface ICasePhotoService
{
    Task<IEnumerable<CasePhotoDto>> GetByCaseIdAsync(Guid caseId);
    Task<(bool Success, string Message, CasePhotoDto? Data)> UploadAsync(Guid caseId, Guid userId, Stream fileStream, string fileName, string contentType, string? caption);
    Task<(bool Success, string Message)> DeleteAsync(Guid photoId);
    Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAsync(Guid photoId);
}
