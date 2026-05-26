namespace Verdiq.Application.DTOs.Reminder;

public class CreateReminderDto
{
    public string Type { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
}

public class ReminderResponseDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool SentStatus { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
