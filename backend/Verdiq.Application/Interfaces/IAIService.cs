namespace Verdiq.Application.Interfaces;

public interface IAIService
{
    Task<(string Reply, int TokensUsed)> ChatAsync(string message, Guid userId);
    Task<string> AnalyzeCaseAsync(Guid caseId);
    Task<string> SummarizeDocumentAsync(Guid documentId);
    Task<string> GenerateLegalNoticeAsync(Guid caseId, string noticeType, string? recipient);
    Task<string> SearchJudgementsAsync(string query, string? court, int? year);
    Task<string> GeneratePetitionAsync(Guid caseId, string petitionType, string? court);
}
