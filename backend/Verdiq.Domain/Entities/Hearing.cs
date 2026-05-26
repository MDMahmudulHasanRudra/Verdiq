using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Hearing : BaseEntity
{
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public DateTime HearingDate { get; set; }
    public string? Courtroom { get; set; }
    public string? JudgeName { get; set; }
    public string? Result { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public HearingStatus Status { get; set; } = HearingStatus.Scheduled;
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; }
}
