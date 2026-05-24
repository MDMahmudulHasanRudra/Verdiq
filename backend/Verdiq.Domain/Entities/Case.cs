using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Case : BaseEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public CaseStatus Status { get; set; } = CaseStatus.Pending;
    public CasePriority Priority { get; set; } = CasePriority.Medium;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public string? ActsAndSections { get; set; }
    public string? Description { get; set; }
    public DateTime FilingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid ClientId { get; set; }
    public Guid AssignedLawyerId { get; set; }
    public Guid OrganizationId { get; set; }

    public Client Client { get; set; } = null!;
    public User AssignedLawyer { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
