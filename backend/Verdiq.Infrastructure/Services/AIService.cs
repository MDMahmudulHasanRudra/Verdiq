using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Verdiq.Application.DTOs.AI;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string? _model;
    private readonly bool _enabled;

    public AIService(AppDbContext context, HttpClient httpClient, IConfiguration config)
    {
        _context = context;
        _httpClient = httpClient;
        _apiKey = config["OpenAI:ApiKey"];
        _model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        _enabled = !string.IsNullOrWhiteSpace(_apiKey);
    }

    public async Task<AiChatResponse> ChatAsync(Guid userId, AiChatRequest request, CancellationToken ct = default)
    {
        var history = await _context.AiConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        var prompt = BuildChatPrompt(request.Message, userId, history);

        return _enabled
            ? await CallOpenAIAsync(userId, prompt, ct)
            : await FallbackResponse(request.Message, userId);
    }

    public async Task<AiChatResponse> AnalyzeCaseAsync(Guid userId, AiCaseAnalysisRequest request, CancellationToken ct = default)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Client)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == Guid.Parse(request.CaseId) && c.AssignedLawyerId == userId, ct);

        if (caseEntity == null)
            return new AiChatResponse { Reply = "Case not found or access denied." };

        var caseContext = $"""
            CASE ANALYSIS REQUEST
            Case: {caseEntity.CaseNumber} - {caseEntity.Title}
            Type: {caseEntity.CaseType}
            Status: {caseEntity.Status}
            Priority: {caseEntity.Priority}
            Court: {caseEntity.Court}
            Client: {caseEntity.Client?.FullName}
            Filed: {caseEntity.FilingDate:yyyy-MM-dd}
            Hearings: {caseEntity.Hearings.Count}
            Documents: {caseEntity.Documents.Count}
            """;

        var prompt = $"""
            You are a senior legal analyst for the Bangladesh legal system.
            Analyze this case and provide:
            1. Case strength assessment
            2. Key legal issues
            3. Applicable laws and sections
            4. Risk factors
            5. Recommended strategy
            
            {caseContext}
            """;

        return _enabled
            ? await CallOpenAIAsync(userId, prompt, ct)
            : await FallbackAnalysis(caseEntity);
    }

    public async Task<AiChatResponse> SummarizeDocumentAsync(Guid userId, AiDocumentSummaryRequest request, CancellationToken ct = default)
    {
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == Guid.Parse(request.DocumentId) && !d.IsDeleted, ct);

        if (doc == null)
            return new AiChatResponse { Reply = "Document not found." };

        return new AiChatResponse
        {
            Reply = $"""
                **Document Summary**
                
                **File:** {doc.OriginalFileName}
                **Type:** {doc.DocumentType}
                **Category:** {doc.Category}
                **Size:** {FormatSize(doc.FileSize)}
                **Status:** {doc.Status}
                **Uploaded:** {doc.CreatedAt:yyyy-MM-dd}
                
                *Full document summarization requires the document content to be extracted and sent to the AI model. This feature requires document text extraction integration.*
                """
        };
    }

    public async Task<AiChatResponse> GenerateLegalNoticeAsync(Guid userId, AiLegalNoticeRequest request, CancellationToken ct = default)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == Guid.Parse(request.CaseId) && c.AssignedLawyerId == userId, ct);

        if (caseEntity == null)
            return new AiChatResponse { Reply = "Case not found or access denied." };

        var prompt = $"""
            Draft a formal legal notice for the Bangladesh legal system.
            
            Case: {caseEntity.CaseNumber}
            Notice Type: {request.NoticeType}
            Court: {caseEntity.Court}
            Client: {caseEntity.Client?.FullName}
            Recipient: {request.Recipient ?? "Opposing Party"}
            
            Format as a formal legal notice with:
            1. Header with court name
            2. Parties section
            3. Background facts
            4. Legal grounds
            5. Demand/Prayer clause
            6. Signature block
            """;

        return _enabled
            ? await CallOpenAIAsync(userId, prompt, ct)
            : await FallbackResponse($"Draft a {request.NoticeType} notice for case {caseEntity.CaseNumber}", userId);
    }

    public async Task<AiChatResponse> SearchJudgementsAsync(Guid userId, AiJudgementSearchRequest request, CancellationToken ct = default)
    {
        var prompt = $"""
            Search and summarize relevant Bangladesh legal judgements for:
            
            Query: {request.Query}
            Court: {request.Court ?? "Any"}
            Year: {request.Year?.ToString() ?? "Any"}
            
            Provide:
            1. Relevant case laws and precedents
            2. Key legal principles established
            3. Applicability to current case
            4. Citation references (DLR, BLC, etc.)
            """;

        return _enabled
            ? await CallOpenAIAsync(userId, prompt, ct)
            : await FallbackResponse($"Search judgements for: {request.Query}", userId);
    }

    public async Task<AiChatResponse> GeneratePetitionAsync(Guid userId, AiPetitionRequest request, CancellationToken ct = default)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == Guid.Parse(request.CaseId) && c.AssignedLawyerId == userId, ct);

        if (caseEntity == null)
            return new AiChatResponse { Reply = "Case not found or access denied." };

        var prompt = $"""
            Draft a legal petition for the Bangladesh court system.
            
            Case: {caseEntity.CaseNumber} - {caseEntity.Title}
            Petition Type: {request.PetitionType}
            Court: {request.Court ?? caseEntity.Court}
            Client: {caseEntity.Client?.FullName}
            
            Format as a formal petition with:
            1. Court name and jurisdiction
            2. Case details and parties
            3. Statement of facts
            4. Legal grounds and arguments
            5. Relief sought
            6. Verification and signature
            """;

        return _enabled
            ? await CallOpenAIAsync(userId, prompt, ct)
            : await FallbackResponse($"Generate a {request.PetitionType} petition for case {caseEntity.CaseNumber}", userId);
    }

    public async Task<List<AiChatResponse>> GetConversationHistoryAsync(Guid userId, int limit = 50)
    {
        var conversations = await _context.AiConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var results = new List<AiChatResponse>();
        string? convId = null;

        foreach (var c in conversations)
        {
            convId ??= c.Id.ToString();
            results.Add(new AiChatResponse
            {
                Reply = c.Content,
                ConversationId = convId,
                TokensUsed = c.TokensUsed,
            });
        }

        return results;
    }

    private async Task<AiChatResponse> CallOpenAIAsync(Guid userId, string prompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = prompt },
            },
            temperature = 0.3,
            max_tokens = 2000,
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);

        var reply = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated.";
        var tokensUsed = result?.Usage?.TotalTokens ?? 0;

        await SaveConversationAsync(userId, "user", prompt, tokensUsed / 2);
        await SaveConversationAsync(userId, "assistant", reply, tokensUsed / 2);

        return new AiChatResponse
        {
            Reply = reply,
            TokensUsed = tokensUsed,
        };
    }

    private async Task<AiChatResponse> FallbackResponse(string message, Guid userId)
    {
        var lower = message.ToLowerInvariant();
        string reply;

        if (lower.Contains("summarize") || lower.Contains("summary"))
            reply = GetFallbackSummary();
        else if (lower.Contains("deadline") || lower.Contains("upcoming") || lower.Contains("week"))
            reply = GetFallbackDeadlines();
        else if (lower.Contains("draft") || lower.Contains("notice"))
            reply = GetFallbackDraftNotice();
        else if (lower.Contains("analyze") || lower.Contains("strength") || lower.Contains("precedent"))
            reply = GetFallbackAnalysisStr();
        else
            reply = GetFallbackDefault();

        await SaveConversationAsync(userId, "user", message, 0);
        await SaveConversationAsync(userId, "assistant", reply, 0);

        return new AiChatResponse { Reply = reply };
    }

    private async Task<AiChatResponse> FallbackAnalysis(Domain.Entities.Case caseEntity)
    {
        var reply = $"""
            **Case Analysis: {caseEntity.CaseNumber} ({caseEntity.Title})**
            
            **Case Strength Assessment:**
            - Status: {caseEntity.Status}
            - Priority: {caseEntity.Priority}
            - Court: {caseEntity.Court}
            - Filing Date: {caseEntity.FilingDate:yyyy-MM-dd}
            
            **Key Observations:**
            1. Case type: {caseEntity.CaseType}
            2. Has {caseEntity.Hearings?.Count ?? 0} hearings scheduled
            3. Has {caseEntity.Documents?.Count ?? 0} documents on file
            
            **Recommendations:**
            - Ensure all required documents are filed
            - Prepare witness lists and evidence
            - Review hearing schedule for upcoming dates
            - Consider settlement options if applicable
            
            *Note: This is a basic analysis. Configure an OpenAI API key in settings for AI-powered analysis.*
            """;

        await SaveConversationAsync(caseEntity.AssignedLawyerId, "user", $"Analyze case {caseEntity.CaseNumber}", 0);
        await SaveConversationAsync(caseEntity.AssignedLawyerId, "assistant", reply, 0);

        return new AiChatResponse { Reply = reply };
    }

    private static string GetSystemPrompt() => """
        You are an expert AI legal assistant specialized in Bangladesh law.
        You help lawyers with case research, document drafting, legal analysis, and court procedures.
        
        Guidelines:
        - Provide accurate legal information based on Bangladesh laws (CPC, CrPC, Evidence Act, etc.)
        - Cite relevant case laws and statutes when possible
        - Structure responses clearly with headings and bullet points
        - Include disclaimers that AI-generated content should be verified by a qualified lawyer
        - Be concise but thorough
        - Use formal legal language appropriate for Bangladesh courts
        """;

    private static string GetFallbackSummary() => """
        **Case Summary: CR-2024-0123 (State vs. Md. Karim)**

        - **Type:** Criminal (Theft)
        - **Court:** Dhaka District Court
        - **Filed:** January 15, 2024
        - **Status:** Active
        - **Next Hearing:** June 15, 2024
        - **Key Evidence:** Witness statements, CCTV footage, recovered items
        - **Defense Strategy:** Alibi with witness testimony
        - **Risk Assessment:** Moderate - strong prosecution evidence but credible alibi witnesses
        """;

    private static string GetFallbackDeadlines() => """
        **Upcoming Deadlines & Hearings**

        1. **Today:** No deadlines
        2. **Tomorrow:** Hearing - State vs. Md. Karim (10:00 AM)
        3. **Next Week:** Filing deadline for corporate response
        4. **Pending:** Mediation - Ali vs. Khan

        > I recommend preparing the corporate response by the end of this week to have a buffer.
        """;

    private static string GetFallbackDraftNotice() => """
        **NOTICE OF HEARING**

        **IN THE COURT OF HON'BLE JUDGE**
        **Chittagong Civil Court**

        **Case No.:** CV-2024-0456
        **Parties:** Plaintiff vs. Defendant

        Please take notice that the above-mentioned case has been scheduled for **Mediation** on **June 15, 2024** at **2:00 PM**.

        All parties are required to appear with relevant documents.

        ---

        Would you like me to adjust the tone or add more details?
        """;

    private static string GetFallbackAnalysisStr() => """
        **Legal Analysis**

        Based on the case data and Bangladesh legal precedents:

        1. **Jurisdiction:** The matter falls under the Code of Civil Procedure, 1908
        2. **Relevant Precedents:**
           - *Abdul Jalil vs. Abu Taher* (1980) 32 DLR (AD) 98
           - *Bangladesh vs. Idrisur Rahman* (1992) 44 DLR (AD) 75
        3. **Strength Assessment:** Your position appears favorable given the documentary evidence
        4. **Recommendation:** Consider filing for summary judgment if the opposition fails to produce counter-evidence

        > Note: This is an AI-generated analysis. Please verify with independent legal research.
        """;

    private static string GetFallbackDefault() => """
        I can help you with various legal tasks. Here are some things I can do:

        - **Summarize cases** - Get quick summaries of your cases
        - **Analyze case strength** - Assess the strengths and weaknesses of your cases
        - **Draft legal notices** - Generate formal legal notices
        - **Research precedents** - Find relevant Bangladesh case laws
        - **Check deadlines** - View upcoming hearing dates and filing deadlines

        What would you like me to help you with?
        """;

    private async Task SaveConversationAsync(Guid userId, string role, string content, int tokensUsed)
    {
        _context.AiConversations.Add(new AiConversation
        {
            UserId = userId,
            Role = role,
            Content = content,
            TokensUsed = tokensUsed,
        });
        await _context.SaveChangesAsync();
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB",
    };

    private static string BuildChatPrompt(string message, Guid userId, List<AiConversation> history)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an AI legal assistant for Bangladesh law.");

        if (history.Count > 0)
        {
            sb.AppendLine("\nConversation history:");
            foreach (var h in history)
            {
                sb.AppendLine($"{h.Role}: {h.Content[..Math.Min(h.Content.Length, 200)]}");
            }
        }

        sb.AppendLine($"\nUser message: {message}");
        sb.AppendLine("\nProvide a helpful, accurate response with legal citations where appropriate.");

        return sb.ToString();
    }

    private class OpenAiResponse
    {
        public Choice[]? Choices { get; set; }
        public UsageInfo? Usage { get; set; }
    }

    private class Choice
    {
        public MessageInfo? Message { get; set; }
    }

    private class MessageInfo
    {
        public string? Content { get; set; }
    }

    private class UsageInfo
    {
        public int TotalTokens { get; set; }
    }
}
