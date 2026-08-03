namespace Verdiq.Domain.Entities;

public class Lead : BaseEntity
{
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CaseType { get; set; }
    public decimal EstimatedValue { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public string Stage { get; set; } = "NewLead";
    public Guid? AssignedLawyerId { get; set; }
    public User? AssignedLawyer { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentsJson { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public int Score { get; set; }
    public DateTime ConvertedAt { get; set; }
    public string? LostReason { get; set; }

    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? CaseId { get; set; }
    public Case? Case { get; set; }
}
