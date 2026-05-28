namespace Verdiq.Application.DTOs.Lead;

public class CreateLeadDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CaseType { get; set; }
    public decimal EstimatedValue { get; set; }
    public string LeadSource { get; set; } = "Direct";
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid? AssignedLawyerId { get; set; }
}

public class UpdateLeadDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
    public string? CaseType { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? LeadSource { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid? AssignedLawyerId { get; set; }
    public string? LostReason { get; set; }
}

public class UpdateLeadStageDto
{
    public string Stage { get; set; } = string.Empty;
    public string? LostReason { get; set; }
}

public class LeadResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CaseType { get; set; }
    public decimal EstimatedValue { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public Guid? AssignedLawyerId { get; set; }
    public string? AssignedLawyerName { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public int Score { get; set; }
    public bool IsStale { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string? LostReason { get; set; }
}

public class LeadAnalyticsDto
{
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int ConsultationScheduled { get; set; }
    public int FollowUpPending { get; set; }
    public int ProposalSent { get; set; }
    public int Converted { get; set; }
    public int Lost { get; set; }
    public double ConversionRate { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public decimal ConvertedValue { get; set; }
    public double AverageConversionDays { get; set; }
    public List<SourceBreakdown> BySource { get; set; } = new();
}

public class SourceBreakdown
{
    public string Source { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Value { get; set; }
}
