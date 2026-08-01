using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Interfaces;

public interface IJudgmentService
{
    Task<IEnumerable<JudgmentDto>> GetByCaseIdAsync(Guid caseId);
    Task<JudgmentDto?> GetByIdAsync(Guid judgmentId);
    Task<(bool Success, string Message, JudgmentDto? Data)> CreateAsync(Guid caseId, CreateJudgmentDto dto, Guid userId);
    Task<(bool Success, string Message)> DeleteAsync(Guid judgmentId);
    Task<(bool Success, string Message, JudgmentDto? Data)> UploadDocumentAsync(Guid caseId, Guid judgmentId, Guid userId, Stream fileStream, string fileName, string contentType);
    Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadDocumentAsync(Guid caseId, Guid judgmentId);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(Guid caseId, string format);
}
