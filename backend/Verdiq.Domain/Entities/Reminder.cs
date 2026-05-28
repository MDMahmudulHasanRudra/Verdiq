using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Reminder : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;

    public string Type { get; set; } = string.Empty;
    public ReminderChannel Channel { get; set; } = ReminderChannel.PushNotification;
    public string Priority { get; set; } = "Medium";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>Case, Task, Hearing, Invoice, Lead, Client, Document, TimeEntry</summary>
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public DateTime ScheduledAt { get; set; }
    public bool SentStatus { get; set; }
    public DateTime? SentAt { get; set; }
    public Guid? ReferenceId { get; set; }

    // Enhanced status fields
    public string Status { get; set; } = "Pending";
    public DateTime? ReadAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SnoozedUntil { get; set; }
    public int EscalationLevel { get; set; }
}
