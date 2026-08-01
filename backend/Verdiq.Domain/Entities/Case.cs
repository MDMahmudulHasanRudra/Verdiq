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

    public string? GdNumber { get; set; }
    public string? JudgeName { get; set; }
    public string? Bench { get; set; }
    public string? Prosecutor { get; set; }
    public string? OpposingLawyer { get; set; }
    public string? Jurisdiction { get; set; }
    public string? AppealStatus { get; set; }
    public string? RiskLevel { get; set; }
    public int? ComplexityScore { get; set; }
    public string? PracticeArea { get; set; }
    public string? Department { get; set; }
    public string? InternalNotes { get; set; }
    public decimal? RetainerAmount { get; set; }
    public string? BillingMethod { get; set; }
    public decimal? FixedFee { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? BudgetLimit { get; set; }
    public decimal? ExpenseBudget { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? CriticalDeadlines { get; set; }
    public DateTime? LimitationExpiry { get; set; }
    public Guid? CaseTemplateId { get; set; }
    public Guid? WorkflowTemplateId { get; set; }
    public virtual WorkflowTemplate? WorkflowTemplate { get; set; }

    public Guid AssignedLawyerId { get; set; }
    public User AssignedLawyer { get; set; } = null!;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<CaseActivity> Activities { get; set; } = new List<CaseActivity>();
    public ICollection<ClientCase> ClientCases { get; set; } = new List<ClientCase>();
    public ICollection<CaseLegalSection> CaseLegalSections { get; set; } = new List<CaseLegalSection>();
    public ICollection<Judgment> Judgments { get; set; } = new List<Judgment>();
    public ICollection<CasePhoto> Photos { get; set; } = new List<CasePhoto>();
    public ICollection<CaseWorkflow> CaseWorkflows { get; set; } = new List<CaseWorkflow>();
}
