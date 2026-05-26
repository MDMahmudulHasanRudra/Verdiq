using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Case : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public string? Opponent { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.Pending;
    public CasePriority Priority { get; set; } = CasePriority.Medium;
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public DateTime? ClosingDate { get; set; }

    public Guid AssignedLawyerId { get; set; }
    public User AssignedLawyer { get; set; } = null!;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<CaseActivity> Activities { get; set; } = new List<CaseActivity>();
    public ICollection<ClientCase> ClientCases { get; set; } = new List<ClientCase>();
}
