using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.AI;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : BaseController
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> Chat(
        [FromBody] AiChatRequest request)
    {
        var userId = GetUserId();
        var (reply, tokensUsed) = await _aiService.ChatAsync(request.Message, userId);
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = reply,
            TokensUsed = tokensUsed,
            ConversationId = request.ConversationId
        }));
    }

    [HttpPost("case-analysis")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> AnalyzeCase(
        [FromBody] AiCaseAnalysisRequest request)
    {
        var result = await _aiService.AnalyzeCaseAsync(Guid.Parse(request.CaseId));
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = result,
            TokensUsed = 0
        }));
    }

    [HttpPost("document-summary")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> SummarizeDocument(
        [FromBody] AiDocumentSummaryRequest request)
    {
        var result = await _aiService.SummarizeDocumentAsync(Guid.Parse(request.DocumentId));
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = result,
            TokensUsed = 0
        }));
    }

    [HttpPost("legal-notice")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> GenerateLegalNotice(
        [FromBody] AiLegalNoticeRequest request)
    {
        var result = await _aiService.GenerateLegalNoticeAsync(
            Guid.Parse(request.CaseId), request.NoticeType, request.Recipient);
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = result,
            TokensUsed = 0
        }));
    }

    [HttpPost("judgement-search")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> SearchJudgements(
        [FromBody] AiJudgementSearchRequest request)
    {
        var result = await _aiService.SearchJudgementsAsync(request.Query, request.Court, request.Year);
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = result,
            TokensUsed = 0
        }));
    }

    [HttpPost("petition-generator")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> GeneratePetition(
        [FromBody] AiPetitionRequest request)
    {
        var result = await _aiService.GeneratePetitionAsync(
            Guid.Parse(request.CaseId), request.PetitionType, request.Court);
        return Ok(ApiResponse<AiChatResponse>.Ok(new AiChatResponse
        {
            Reply = result,
            TokensUsed = 0
        }));
    }
}
