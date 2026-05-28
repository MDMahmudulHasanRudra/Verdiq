namespace Verdiq.Application.DTOs.Reminder;

public class CreateReminderDto
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = "PushNotification";
    public DateTime? ScheduledAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}

public class UpdateReminderStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class SnoozeReminderDto
{
    public DateTime SnoozedUntil { get; set; }
}

public class ReminderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public bool SentStatus { get; set; }
    public DateTime? SentAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SnoozedUntil { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReminderAnalyticsDto
{
    public int TotalPending { get; set; }
    public int TotalRead { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalSnoozed { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int DueToday { get; set; }
    public int OverdueCount { get; set; }
    public double CompletionRate { get; set; }
    public List<ReminderByType> ByType { get; set; } = new();
    public List<ReminderTrend> DailyTrend { get; set; } = new();
}

public class ReminderByType
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Completed { get; set; }
    public int Overdue { get; set; }
}

public class ReminderTrend
{
    public string Date { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Completed { get; set; }
}

public class DailyAgendaDto
{
    public DateTime Date { get; set; }
    public List<ReminderResponseDto> OverdueReminders { get; set; } = new();
    public List<ReminderResponseDto> DueToday { get; set; } = new();
    public List<ReminderResponseDto> UpcomingHearings { get; set; } = new();
    public int TotalPendingTasks { get; set; }
    public int OverdueInvoices { get; set; }
    public int LeadsToFollowUp { get; set; }
    public double BillableHoursToday { get; set; }
}

public class BulkReminderActionDto
{
    public List<Guid> Ids { get; set; } = new();
}

public class ReminderSettingsDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool WhatsAppNotifications { get; set; }
    public string DailyDigestTime { get; set; } = "08:00";
    public bool DailyDigestEnabled { get; set; } = true;
    public bool WeeklyDigestEnabled { get; set; } = true;
    public string QuietHoursStart { get; set; } = "22:00";
    public string QuietHoursEnd { get; set; } = "07:00";
    public int EscalationDelayHours { get; set; } = 24;
    public bool HearingReminders { get; set; } = true;
    public bool TaskReminders { get; set; } = true;
    public bool InvoiceReminders { get; set; } = true;
    public bool LeadFollowUpReminders { get; set; } = true;
    public bool CaseInactivityReminders { get; set; } = true;
    public bool TimesheetReminders { get; set; } = true;
}
