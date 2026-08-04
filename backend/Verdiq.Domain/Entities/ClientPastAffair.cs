namespace Verdiq.Domain.Entities;

public class ClientPastAffair : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

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

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
}
