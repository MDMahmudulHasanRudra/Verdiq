using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Reminder : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public ReminderChannel Channel { get; set; } = ReminderChannel.PushNotification;
    public DateTime ScheduledAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool SentStatus { get; set; }
    public DateTime? SentAt { get; set; }
    public Guid? ReferenceId { get; set; }
}
