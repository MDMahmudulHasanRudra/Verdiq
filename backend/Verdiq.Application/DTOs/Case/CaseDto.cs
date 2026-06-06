namespace Verdiq.Application.DTOs.Case;

public class CreateCaseDto
{
    public string Title { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public string? Opponent { get; set; }
    public string? Priority { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public Guid? AssignedLawyerId { get; set; }
    public Guid? TeamId { get; set; }
    public List<Guid> ClientIds { get; set; } = new();

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
    public List<Guid>? LegalSectionIds { get; set; }
    public List<ClientRoleDto>? ClientRoles { get; set; }
}

public class ClientRoleDto
{
    public Guid ClientId { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UpdateCaseDto
{
    public string? Title { get; set; }
    public string? CourtName { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Opponent { get; set; }
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
    public Guid? AssignedLawyerId { get; set; }
    public Guid? TeamId { get; set; }
    public List<Guid>? ClientIds { get; set; }
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
    public List<Guid>? LegalSectionIds { get; set; }
    public List<ClientRoleDto>? ClientRoles { get; set; }
}

public class CaseResponseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public string? Opponent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActsAndSections { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid AssignedLawyerId { get; set; }
    public string AssignedLawyerName { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public List<ClientInfo> Clients { get; set; } = new();
    public int HearingsCount { get; set; }
    public int DocumentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FirNumber { get; set; }
    public string? PoliceStation { get; set; }
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
    public List<LegalSectionInfo> LegalSections { get; set; } = new();
}

public class ClientInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Role { get; set; }
}

public class LegalSectionInfo
{
    public Guid Id { get; set; }
    public Guid LegalSectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string LawName { get; set; } = string.Empty;
    public List<CaseProcedureInfo> Procedures { get; set; } = new();
}

public class BulkStatusChangeDto
{
    public List<Guid> CaseIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BulkDeleteDto
{
    public List<Guid> CaseIds { get; set; } = new();
}

public class CaseProcedureInfo
{
    public Guid Id { get; set; }
    public Guid LegalProcedureId { get; set; }
    public string ProcedureTitle { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public string? RecommendedTimeline { get; set; }
    public string? ResponsibleRole { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
}