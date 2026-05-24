using Verdiq.Application.DTOs.AI;

namespace Verdiq.Application.Interfaces;

public interface IAIService
{
    Task<AiChatResponse> ChatAsync(Guid userId, AiChatRequest request, CancellationToken ct = default);
    Task<AiChatResponse> AnalyzeCaseAsync(Guid userId, AiCaseAnalysisRequest request, CancellationToken ct = default);
    Task<AiChatResponse> SummarizeDocumentAsync(Guid userId, AiDocumentSummaryRequest request, CancellationToken ct = default);
    Task<AiChatResponse> GenerateLegalNoticeAsync(Guid userId, AiLegalNoticeRequest request, CancellationToken ct = default);
    Task<AiChatResponse> SearchJudgementsAsync(Guid userId, AiJudgementSearchRequest request, CancellationToken ct = default);
    Task<AiChatResponse> GeneratePetitionAsync(Guid userId, AiPetitionRequest request, CancellationToken ct = default);
    Task<List<AiChatResponse>> GetConversationHistoryAsync(Guid userId, int limit = 50);
}
