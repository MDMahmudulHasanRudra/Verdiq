namespace Verdiq.Application.DTOs.AI;

public class AiChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

public class AiChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public int TokensUsed { get; set; }
}

public class AiCaseAnalysisRequest
{
    public string CaseId { get; set; } = string.Empty;
}

public class AiDocumentSummaryRequest
{
    public string DocumentId { get; set; } = string.Empty;
}

public class AiLegalNoticeRequest
{
    public string CaseId { get; set; } = string.Empty;
    public string NoticeType { get; set; } = string.Empty;
    public string? Recipient { get; set; }
}

public class AiJudgementSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Court { get; set; }
    public int? Year { get; set; }
}

public class AiPetitionRequest
{
    public string CaseId { get; set; } = string.Empty;
    public string PetitionType { get; set; } = string.Empty;
    public string? Court { get; set; }
}
