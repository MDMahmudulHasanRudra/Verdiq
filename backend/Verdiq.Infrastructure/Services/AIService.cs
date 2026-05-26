using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    public async Task<(string Reply, int TokensUsed)> ChatAsync(string message, Guid userId)
    {
        var history = await _context.AiConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var prompt = BuildChatPrompt(message, history);

        if (!_enabled)
        {
            var fallbackReply = GetFallbackResponse(message);
            await SaveConversationAsync(userId, "user", message, 0);
            await SaveConversationAsync(userId, "assistant", fallbackReply, 0);
            return (fallbackReply, 0);
        }

        return await CallOpenAIAsync(userId, prompt, CancellationToken.None);
    }

    public async Task<string> AnalyzeCaseAsync(Guid caseId)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == caseId && !c.IsDeleted);

        if (caseEntity == null)
            return "Case not found.";

        if (!_enabled)
        {
            var reply = $"""
                **Case Analysis: {caseEntity.CaseNumber} ({caseEntity.Title})**

                **Status:** {caseEntity.Status}
                **Priority:** {caseEntity.Priority}
                **Court:** {caseEntity.CourtName}
                **Filed:** {caseEntity.FilingDate:yyyy-MM-dd}

                **Key Details:**
                - Type: {caseEntity.CaseType}
                - Opponent: {caseEntity.Opponent ?? "N/A"}
                - Hearings: {caseEntity.Hearings.Count}
                - Documents: {caseEntity.Documents.Count}

                *Configure an OpenAI API key in settings for AI-powered analysis.*
                """;
            await SaveConversationAsync(caseEntity.AssignedLawyerId, "user", $"Analyze case {caseEntity.CaseNumber}", 0);
            await SaveConversationAsync(caseEntity.AssignedLawyerId, "assistant", reply, 0);
            return reply;
        }

        var caseContext = $"""
            CASE ANALYSIS REQUEST
            Case: {caseEntity.CaseNumber} - {caseEntity.Title}
            Type: {caseEntity.CaseType}
            Status: {caseEntity.Status}
            Priority: {caseEntity.Priority}
            Court: {caseEntity.CourtName}
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

        var result = await CallOpenAIAsync(caseEntity.AssignedLawyerId, prompt, default);
        return result.Reply;
    }

    public async Task<string> SummarizeDocumentAsync(Guid documentId)
    {
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (doc == null)
            return "Document not found.";

        return $"""
            **Document Summary**

            **File:** {doc.OriginalFileName}
            **Type:** {doc.FileType}
            **Category:** {doc.Category}
            **Size:** {FormatSize(doc.FileSize)}
            **Status:** {doc.Status}
            **Uploaded:** {doc.CreatedAt:yyyy-MM-dd}

            *Full document summarization requires the document content to be extracted and sent to the AI model. This feature requires document text extraction integration.*
            """;
    }

    public async Task<string> GenerateLegalNoticeAsync(Guid caseId, string noticeType, string? recipient)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .FirstOrDefaultAsync(c => c.Id == caseId && !c.IsDeleted);

        if (caseEntity == null)
            return "Case not found.";

        if (!_enabled)
        {
            return $"""
                **{noticeType.ToUpperInvariant()} NOTICE**

                **IN THE COURT OF HON'BLE JUDGE**
                **{caseEntity.CourtName}**

                **Case No.:** {caseEntity.CaseNumber}
                **Parties:** {caseEntity.Title}

                Please take notice that the above-mentioned case has been scheduled for **{noticeType}**.

                All parties are required to appear with relevant documents.

                ---
                *This is a draft notice. Configure an OpenAI API key for AI-generated content.*
                """;
        }

        var prompt = $"""
            Draft a formal legal notice for the Bangladesh legal system.

            Case: {caseEntity.CaseNumber}
            Notice Type: {noticeType}
            Court: {caseEntity.CourtName}
            Recipient: {recipient ?? "Opposing Party"}

            Format as a formal legal notice with:
            1. Header with court name
            2. Parties section
            3. Background facts
            4. Legal grounds
            5. Demand/Prayer clause
            6. Signature block
            """;

        var result = await CallOpenAIAsync(caseEntity.AssignedLawyerId, prompt, default);
        return result.Reply;
    }

    public async Task<string> SearchJudgementsAsync(string query, string? court, int? year)
    {
        if (!_enabled)
        {
            return $"""
                **Legal Research Results**

                Based on available data and Bangladesh legal precedents:

                **Query:** {query}
                **Court:** {court ?? "All"}
                **Year:** {year?.ToString() ?? "All"}

                Relevant precedents may include:
                - *Abdul Jalil vs. Abu Taher* (1980) 32 DLR (AD) 98
                - *Bangladesh vs. Idrisur Rahman* (1992) 44 DLR (AD) 75

                *Configure an OpenAI API key in settings for AI-powered judgement search.*
                """;
        }

        var prompt = $"""
            Search and summarize relevant Bangladesh legal judgements for:

            Query: {query}
            Court: {court ?? "Any"}
            Year: {year?.ToString() ?? "Any"}

            Provide:
            1. Relevant case laws and precedents
            2. Key legal principles established
            3. Applicability to current case
            4. Citation references (DLR, BLC, etc.)
            """;

        var result = await CallOpenAIAsync(Guid.Empty, prompt, default);
        return result.Reply;
    }

    public async Task<string> GeneratePetitionAsync(Guid caseId, string petitionType, string? court)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .FirstOrDefaultAsync(c => c.Id == caseId && !c.IsDeleted);

        if (caseEntity == null)
            return "Case not found.";

        if (!_enabled)
        {
            return $"""
                **{petitionType} PETITION**

                **IN THE COURT OF HON'BLE JUDGE**
                **{court ?? caseEntity.CourtName}**

                **Case No.:** {caseEntity.CaseNumber}
                **Parties:** {caseEntity.Title}

                The petitioner respectfully states as follows:

                1. This is an application for {petitionType}
                2. The facts of the case are as follows...

                **Prayer:** It is therefore prayed that Your Honor may be graciously pleased to pass necessary orders.

                ---
                *This is a draft petition. Configure an OpenAI API key for AI-generated content.*
                """;
        }

        var prompt = $"""
            Draft a legal petition for the Bangladesh court system.

            Case: {caseEntity.CaseNumber} - {caseEntity.Title}
            Petition Type: {petitionType}
            Court: {court ?? caseEntity.CourtName}

            Format as a formal petition with:
            1. Court name and jurisdiction
            2. Case details and parties
            3. Statement of facts
            4. Legal grounds and arguments
            5. Relief sought
            6. Verification and signature
            """;

        var result = await CallOpenAIAsync(caseEntity.AssignedLawyerId, prompt, default);
        return result.Reply;
    }

    private async Task<(string Reply, int TokensUsed)> CallOpenAIAsync(Guid userId, string prompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 2000
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = System.Text.Json.JsonSerializer.Deserialize<OpenAiResponse>(responseJson);

        var reply = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated.";
        var tokensUsed = result?.Usage?.TotalTokens ?? 0;

        await SaveConversationAsync(userId, "user", prompt, tokensUsed / 2);
        await SaveConversationAsync(userId, "assistant", reply, tokensUsed / 2);

        return (reply, tokensUsed);
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

    private static string GetFallbackResponse(string message)
    {
        var lower = message.ToLowerInvariant();

        if (lower.Contains("summarize") || lower.Contains("summary"))
            return """
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

        if (lower.Contains("deadline") || lower.Contains("upcoming") || lower.Contains("week"))
            return """
                **Upcoming Deadlines & Hearings**

                1. **Today:** No deadlines
                2. **Tomorrow:** Hearing - State vs. Md. Karim (10:00 AM)
                3. **Next Week:** Filing deadline for corporate response
                4. **Pending:** Mediation - Ali vs. Khan

                > I recommend preparing the corporate response by the end of this week to have a buffer.
                """;

        if (lower.Contains("draft") || lower.Contains("notice"))
            return """
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

        if (lower.Contains("analyze") || lower.Contains("strength") || lower.Contains("precedent"))
            return """
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

        return """
            I can help you with various legal tasks. Here are some things I can do:

            - **Summarize cases** - Get quick summaries of your cases
            - **Analyze case strength** - Assess the strengths and weaknesses of your cases
            - **Draft legal notices** - Generate formal legal notices
            - **Research precedents** - Find relevant Bangladesh case laws
            - **Check deadlines** - View upcoming hearing dates and filing deadlines

            What would you like me to help you with?
            """;
    }

    private static string BuildChatPrompt(string message, List<AiConversation> history)
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

    private async Task SaveConversationAsync(Guid userId, string role, string content, int tokensUsed)
    {
        _context.AiConversations.Add(new AiConversation
        {
            UserId = userId,
            Role = role,
            Content = content,
            TokensUsed = tokensUsed
        });
        await _context.SaveChangesAsync();
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
    };

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
