namespace Verdiq.Application.DTOs.Client;

public class CreateClientPastAffairDto
{
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public string? CourtName { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Opponent { get; set; }
    public string? JudgeName { get; set; }
    public string? Verdict { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? LawyerName { get; set; }
    public bool IsCriminal { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
}

public class UpdateClientPastAffairDto
{
    public string? CaseTitle { get; set; }
    public string? CaseNumber { get; set; }
    public string? CourtName { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Opponent { get; set; }
    public string? JudgeName { get; set; }
    public string? Verdict { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? LawyerName { get; set; }
    public bool? IsCriminal { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
}

public class ClientPastAffairResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public string? CourtName { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public DateTime? FilingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Opponent { get; set; }
    public string? JudgeName { get; set; }
    public string? Verdict { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? LawyerName { get; set; }
    public bool IsCriminal { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    public int DocumentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
