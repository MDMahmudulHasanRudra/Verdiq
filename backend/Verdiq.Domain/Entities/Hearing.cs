using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Hearing : BaseEntity
{
    public Guid CaseId { get; set; }
    public DateTime HearingDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? CourtRoom { get; set; }
    public string? JudgeName { get; set; }
    public string HearingType { get; set; } = string.Empty;
    public HearingStatus Status { get; set; } = HearingStatus.Scheduled;
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; }
    public Guid OrganizationId { get; set; }

    public Case Case { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
