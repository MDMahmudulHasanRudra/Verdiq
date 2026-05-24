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
        [FromBody] AiChatRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.ChatAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpPost("case-analysis")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> AnalyzeCase(
        [FromBody] AiCaseAnalysisRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.AnalyzeCaseAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpPost("document-summary")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> SummarizeDocument(
        [FromBody] AiDocumentSummaryRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.SummarizeDocumentAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpPost("legal-notice")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> GenerateLegalNotice(
        [FromBody] AiLegalNoticeRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.GenerateLegalNoticeAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpPost("judgement-search")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> SearchJudgements(
        [FromBody] AiJudgementSearchRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.SearchJudgementsAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpPost("petition-generator")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> GeneratePetition(
        [FromBody] AiPetitionRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _aiService.GeneratePetitionAsync(userId, request, ct);
        return Ok(ApiResponse<AiChatResponse>.Ok(result));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<AiChatResponse>>>> GetHistory(
        [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var history = await _aiService.GetConversationHistoryAsync(userId, limit);
        return Ok(ApiResponse<List<AiChatResponse>>.Ok(history));
    }
}
