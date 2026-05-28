using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Reminder;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;
using DomainTask = Verdiq.Domain.Entities.Task;
using DomainTaskStatus = Verdiq.Domain.Enums.TaskStatus;

namespace Verdiq.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _context;

    public ReminderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReminderResponseDto> CreateAsync(CreateReminderDto dto, Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var reminder = new Reminder
        {
            ChamberId = chamberId,
            UserId = dto.UserId,
            Type = dto.Type,
            Priority = dto.Priority,
            Title = dto.Title,
            Message = dto.Message,
            Channel = Enum.TryParse<ReminderChannel>(dto.Channel, true, out var channel) ? channel : ReminderChannel.PushNotification,
            ScheduledAt = dto.ScheduledAt.HasValue ? DateTime.SpecifyKind(dto.ScheduledAt.Value, DateTimeKind.Utc) : now,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            Status = "Pending",
            SentStatus = false,
            CreatedAt = now,
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(reminder.Id, chamberId))!;
    }

    public async Task<ReminderResponseDto?> GetByIdAsync(Guid id, Guid chamberId)
    {
        var reminder = await _context.Reminders.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && r.ChamberId == chamberId && !r.IsDeleted);
        return reminder == null ? null : MapToDto(reminder);
    }

    public async Task<IEnumerable<ReminderResponseDto>> GetAllAsync(Guid chamberId, string? status = null, string? type = null, string? priority = null)
    {
        var query = _context.Reminders.Include(r => r.User).Where(r => r.ChamberId == chamberId && !r.IsDeleted);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(type)) query = query.Where(r => r.Type == type);
        if (!string.IsNullOrEmpty(priority)) query = query.Where(r => r.Priority == priority);
        return (await query.OrderByDescending(r => r.Priority == "Critical" ? 0 : r.Priority == "High" ? 1 : r.Priority == "Medium" ? 2 : 3)
            .ThenBy(r => r.ScheduledAt).ToListAsync()).Select(MapToDto);
    }

    public async Task<IEnumerable<ReminderResponseDto>> GetMyRemindersAsync(Guid userId, string? status = null)
    {
        var now = DateTime.UtcNow;
        var query = _context.Reminders.Include(r => r.User).Where(r => r.UserId == userId && !r.IsDeleted);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        else
            query = query.Where(r => r.Status != "Completed" && r.Status != "Dismissed" && (r.SnoozedUntil == null || r.SnoozedUntil <= now));
        return (await query.OrderByDescending(r => r.Priority == "Critical" ? 0 : 1).ThenBy(r => r.ScheduledAt).ToListAsync()).Select(MapToDto);
    }

    public async Task<ReminderResponseDto?> UpdateStatusAsync(Guid id, UpdateReminderStatusDto dto, Guid chamberId)
    {
        var reminder = await _context.Reminders.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id && r.ChamberId == chamberId && !r.IsDeleted);
        if (reminder == null) return null;
        var now = DateTime.UtcNow;
        reminder.Status = dto.Status; reminder.UpdatedAt = now;
        if (dto.Status == "Read" && reminder.ReadAt == null) reminder.ReadAt = now;
        if (dto.Status == "Completed") reminder.CompletedAt = now;
        if (dto.Status == "Dismissed") reminder.CompletedAt = now;
        await _context.SaveChangesAsync();
        return MapToDto(reminder);
    }

    public async Task<ReminderResponseDto?> SnoozeAsync(Guid id, SnoozeReminderDto dto, Guid chamberId)
    {
        var reminder = await _context.Reminders.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id && r.ChamberId == chamberId && !r.IsDeleted);
        if (reminder == null) return null;
        reminder.Status = "Snoozed"; reminder.SnoozedUntil = DateTime.SpecifyKind(dto.SnoozedUntil, DateTimeKind.Utc); reminder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToDto(reminder);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid chamberId)
    {
        var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.ChamberId == chamberId && !r.IsDeleted);
        if (reminder == null) return false;
        reminder.IsDeleted = true; reminder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(); return true;
    }

    public async Task<bool> BulkMarkReadAsync(List<Guid> ids, Guid chamberId)
    {
        var reminders = await _context.Reminders.Where(r => ids.Contains(r.Id) && r.ChamberId == chamberId && !r.IsDeleted).ToListAsync();
        if (reminders.Count == 0) return false;
        var now = DateTime.UtcNow;
        foreach (var r in reminders) { r.Status = "Read"; r.ReadAt ??= now; r.UpdatedAt = now; }
        await _context.SaveChangesAsync(); return true;
    }

    public async Task<bool> BulkCompleteAsync(List<Guid> ids, Guid chamberId)
    {
        var reminders = await _context.Reminders.Where(r => ids.Contains(r.Id) && r.ChamberId == chamberId && !r.IsDeleted).ToListAsync();
        if (reminders.Count == 0) return false;
        var now = DateTime.UtcNow;
        foreach (var r in reminders) { r.Status = "Completed"; r.CompletedAt ??= now; r.UpdatedAt = now; }
        await _context.SaveChangesAsync(); return true;
    }

    public async Task<bool> BulkDeleteAsync(List<Guid> ids, Guid chamberId)
    {
        var reminders = await _context.Reminders.Where(r => ids.Contains(r.Id) && r.ChamberId == chamberId && !r.IsDeleted).ToListAsync();
        if (reminders.Count == 0) return false;
        foreach (var r in reminders) { r.IsDeleted = true; r.UpdatedAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync(); return true;
    }

    public async Task<ReminderAnalyticsDto> GetAnalyticsAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var reminders = await _context.Reminders.Where(r => r.ChamberId == chamberId && !r.IsDeleted).ToListAsync();
        var total = reminders.Count; var completed = reminders.Count(r => r.Status == "Completed");
        return new ReminderAnalyticsDto
        {
            TotalPending = reminders.Count(r => r.Status == "Pending"), TotalRead = reminders.Count(r => r.Status == "Read"),
            TotalCompleted = completed, TotalSnoozed = reminders.Count(r => r.Status == "Snoozed"),
            CriticalCount = reminders.Count(r => r.Priority == "Critical"), HighCount = reminders.Count(r => r.Priority == "High"),
            DueToday = reminders.Count(r => r.ScheduledAt.Date == now.Date && r.Status == "Pending"),
            OverdueCount = reminders.Count(r => r.ScheduledAt < now && r.Status == "Pending"),
            CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
            ByType = reminders.GroupBy(r => r.Type).Select(g => new ReminderByType { Type = g.Key, Count = g.Count(), Completed = g.Count(r => r.Status == "Completed"), Overdue = g.Count(r => r.ScheduledAt < now && r.Status == "Pending") }).OrderByDescending(x => x.Count).ToList(),
            DailyTrend = reminders.GroupBy(r => r.CreatedAt.Date).Select(g => new ReminderTrend { Date = g.Key.ToString("yyyy-MM-dd"), Created = g.Count(), Completed = g.Count(r => r.Status == "Completed") }).OrderBy(x => x.Date).ToList(),
        };
    }

    public async Task<DailyAgendaDto> GetDailyAgendaAsync(Guid userId, Guid chamberId)
    {
        var now = DateTime.UtcNow; var todayStart = now.Date; var todayEnd = todayStart.AddDays(1);
        var myReminders = await _context.Reminders.Where(r => r.ChamberId == chamberId && !r.IsDeleted && r.UserId == userId).ToListAsync();

        var dueToday = myReminders.Where(r => r.ScheduledAt >= todayStart && r.ScheduledAt < todayEnd && r.Status == "Pending")
            .OrderBy(r => r.Priority == "Critical" ? 0 : 1).ThenBy(r => r.ScheduledAt).Select(MapToDto).ToList();
        var overdue = myReminders.Where(r => r.ScheduledAt < todayStart && r.Status == "Pending")
            .OrderByDescending(r => r.Priority == "Critical" ? 0 : 1).ThenBy(r => r.ScheduledAt).Select(MapToDto).ToList();

        var upcomingHearings = await _context.Set<Hearing>().Include(h => h.Case)
            .Where(h => h.HearingDate >= todayStart && h.HearingDate < todayEnd && h.Case != null && h.Case.ChamberId == chamberId && h.Case.AssignedLawyerId == userId && !h.IsDeleted)
            .Select(h => new ReminderResponseDto { Id = h.Id, UserId = userId, Type = "Upcoming Hearing", Priority = "High", Title = "Hearing Today", Message = $"Hearing at {h.HearingDate:hh:mm tt}", RelatedEntityType = "Hearing", RelatedEntityId = h.Id, ScheduledAt = h.HearingDate, Status = "Pending", CreatedAt = h.CreatedAt }).ToListAsync();

        var totalPendingTasks = await _context.Set<DomainTask>()
            .CountAsync(t => t.ChamberId == chamberId && !t.IsDeleted && t.AssignedTo == userId && t.Status == DomainTaskStatus.Pending);
        var overdueInvoices = await _context.Set<Invoice>().CountAsync(i => i.DueDate != null && i.DueDate < todayStart && i.Status == PaymentStatus.Pending && !i.IsDeleted && _context.Set<Case>().Any(c => c.Id == i.CaseId && c.ChamberId == chamberId));
        var leadsToFollowUp = await _context.Set<Lead>().CountAsync(l => l.ChamberId == chamberId && !l.IsDeleted && l.AssignedLawyerId == userId && l.Stage != "ConvertedToClient" && l.Stage != "LostLead" && l.LastContactedAt != null && l.LastContactedAt < now.AddDays(-3));
        var billableToday = await _context.Set<TimeEntry>().Where(t => t.ChamberId == chamberId && !t.IsDeleted && t.UserId == userId && t.Billable && t.StartTime >= todayStart && t.StartTime < todayEnd).SumAsync(t => t.DurationMinutes);

        return new DailyAgendaDto { Date = now, OverdueReminders = overdue, DueToday = dueToday, UpcomingHearings = upcomingHearings, TotalPendingTasks = totalPendingTasks, OverdueInvoices = overdueInvoices, LeadsToFollowUp = leadsToFollowUp, BillableHoursToday = Math.Round(billableToday / 60, 2) };
    }

    public async Task<ReminderResponseDto?> GetNextUpcomingAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var reminder = await _context.Reminders.Where(r => r.UserId == userId && !r.IsDeleted && r.Status == "Pending" && r.ScheduledAt >= now)
            .OrderBy(r => r.Priority == "Critical" ? 0 : 1).ThenBy(r => r.ScheduledAt).FirstOrDefaultAsync();
        return reminder == null ? null : MapToDto(reminder);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _context.Reminders.CountAsync(r => r.UserId == userId && !r.IsDeleted && r.Status == "Pending");

    public async Task EvaluateAutomationRulesAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var users = await _context.Users.Where(u => u.ChamberId == chamberId && u.IsActive && !u.IsDeleted).ToListAsync();
        foreach (var user in users)
        {
            await EvaluateHearingReminders(user, chamberId, now);
            await EvaluateOverdueTasks(user, chamberId, now);
            await EvaluateInvoiceReminders(user, chamberId, now);
            await EvaluateLeadFollowUp(user, chamberId, now);
            await EvaluateCaseInactivity(user, chamberId, now);
            await EvaluateTimesheetCompliance(user, chamberId, now);
        }
    }

    public async Task EvaluateAllChambersAsync()
    {
        var chamberIds = await _context.Chambers.Where(c => !c.IsDeleted).Select(c => c.Id).ToListAsync();
        foreach (var chamberId in chamberIds) await EvaluateAutomationRulesAsync(chamberId);
    }

    private async Task EvaluateHearingReminders(User user, Guid chamberId, DateTime now)
    {
        var hearings = await _context.Set<Hearing>().Include(h => h.Case)
            .Where(h => h.Case != null && h.Case.ChamberId == chamberId && h.Case.AssignedLawyerId == user.Id && !h.IsDeleted && h.HearingDate > now).ToListAsync();
        foreach (var hearing in hearings)
        {
            var hrs = (hearing.HearingDate - now).TotalHours;
            if (hrs <= 0.5 && hrs > 0) await CreateIfNotExists("Upcoming Hearing", "High", user.Id, chamberId, "Hearing in 30 minutes", "Hearing starts in 30 minutes", "Hearing", hearing.Id, hearing.HearingDate);
            else if (hrs <= 2 && hrs > 1.5) await CreateIfNotExists("Upcoming Hearing", "High", user.Id, chamberId, "Hearing in 2 hours", "Hearing starts in 2 hours", "Hearing", hearing.Id, hearing.HearingDate);
            else if (hrs <= 24 && hrs > 23) await CreateIfNotExists("Upcoming Hearing", "Medium", user.Id, chamberId, "Hearing tomorrow", $"Hearing is tomorrow at {hearing.HearingDate:hh:mm tt}", "Hearing", hearing.Id, hearing.HearingDate);
        }
    }

    private async Task EvaluateOverdueTasks(User user, Guid chamberId, DateTime now)
    {
        var overdueTasks = await _context.Set<DomainTask>()
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted && t.AssignedTo == user.Id && t.Status == DomainTaskStatus.Pending && t.DueDate < now).ToListAsync();
        foreach (var task in overdueTasks)
        {
            var hrs = (now - task.DueDate).TotalHours;
            var priority = hrs > 48 ? "Critical" : hrs > 24 ? "High" : "Medium";
            await CreateIfNotExists("Overdue Task", priority, user.Id, chamberId, hrs > 48 ? "Task critically overdue" : hrs > 24 ? "Task still overdue" : "Task overdue", $"\"{task.Title}\" is {(hrs > 48 ? "over 48h" : hrs > 24 ? "over 24h" : "overdue")}", "Task", task.Id, task.DueDate);
        }
    }

    private async Task EvaluateInvoiceReminders(User user, Guid chamberId, DateTime now)
    {
        var overdueInvoices = await _context.Set<Invoice>().Include(i => i.Case)
            .Where(i => i.Case != null && i.Case.ChamberId == chamberId && !i.IsDeleted && i.DueDate != null && i.DueDate < now && i.Status == PaymentStatus.Pending).ToListAsync();
        foreach (var invoice in overdueInvoices)
        {
            var days = (now - invoice.DueDate!.Value).TotalDays;
            var priority = days > 14 ? "Critical" : days > 7 ? "High" : "Medium";
            await CreateIfNotExists("Invoice Overdue", priority, user.Id, chamberId, days > 14 ? "Invoice critically overdue" : days > 7 ? "Invoice overdue - escalate" : "Invoice due", $"Invoice #{invoice.InvoiceNumber} is {days:F0} days overdue (${invoice.Amount})", "Invoice", invoice.Id, invoice.DueDate!.Value);
        }
    }

    private async Task EvaluateLeadFollowUp(User user, Guid chamberId, DateTime now)
    {
        var staleLeads = await _context.Set<Lead>().Where(l => l.ChamberId == chamberId && !l.IsDeleted && l.AssignedLawyerId == user.Id && l.Stage != "ConvertedToClient" && l.Stage != "LostLead" && l.LastContactedAt != null).ToListAsync();
        foreach (var lead in staleLeads)
        {
            var days = (now - lead.LastContactedAt!.Value).TotalDays;
            if (days > 7) await CreateIfNotExists("Lead Follow-up", "High", user.Id, chamberId, "Lead stale - urgent follow-up needed", $"{lead.Name} hasn't been contacted in {days:F0} days (value: ${lead.EstimatedValue})", "Lead", lead.Id, now);
            else if (days > 3) await CreateIfNotExists("Lead Follow-up", "Medium", user.Id, chamberId, "Lead follow-up needed", $"{lead.Name} hasn't been contacted in {days:F0} days", "Lead", lead.Id, now);
        }
    }

    private async Task EvaluateCaseInactivity(User user, Guid chamberId, DateTime now)
    {
        var cases = await _context.Set<Case>().Where(c => c.ChamberId == chamberId && !c.IsDeleted && c.AssignedLawyerId == user.Id && c.Status != CaseStatus.Closed).ToListAsync();
        foreach (var c in cases)
        {
            var last = c.UpdatedAt ?? c.CreatedAt;
            var days = (now - last).TotalDays;
            if (days > 30) await CreateIfNotExists("Inactive Case", "Critical", user.Id, chamberId, "Case inactive for over 30 days", $"\"{c.Title}\" has had no activity for {days:F0} days", "Case", c.Id, now);
            else if (days > 14) await CreateIfNotExists("Inactive Case", "Medium", user.Id, chamberId, "Case inactive for 14+ days", $"\"{c.Title}\" has had no activity for {days:F0} days", "Case", c.Id, now);
        }
    }

    private async Task EvaluateTimesheetCompliance(User user, Guid chamberId, DateTime now)
    {
        var todayStart = now.Date;
        var todayMinutes = await _context.Set<TimeEntry>().Where(t => t.ChamberId == chamberId && t.UserId == user.Id && t.StartTime >= todayStart && t.StartTime < todayStart.AddDays(1)).SumAsync(t => t.DurationMinutes);
        if (todayMinutes == 0 && now.Hour >= 18)
            await CreateIfNotExists("Missing Time Entry", "Medium", user.Id, chamberId, "No time logged today", "You haven't logged any billable hours today.", "TimeEntry", null, now);

        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var weekMinutes = await _context.Set<TimeEntry>().Where(t => t.ChamberId == chamberId && t.UserId == user.Id && t.StartTime >= weekStart).SumAsync(t => t.DurationMinutes);
        if (weekMinutes / 60 < 20 && now.DayOfWeek == DayOfWeek.Sunday && now.Hour >= 16)
            await CreateIfNotExists("Missing Time Entry", "High", user.Id, chamberId, "Low billable hours this week", $"You've logged only {weekMinutes / 60:F0}h this week. Target: 20h.", "TimeEntry", null, now);
    }

    private async Task CreateIfNotExists(string type, string priority, Guid userId, Guid chamberId, string title, string message, string? entityType, Guid? entityId, DateTime scheduledAt)
    {
        if (await _context.Reminders.AnyAsync(r => r.ChamberId == chamberId && r.UserId == userId && r.Type == type && r.RelatedEntityId == entityId && r.Status == "Pending" && !r.IsDeleted)) return;
        _context.Reminders.Add(new Reminder { ChamberId = chamberId, UserId = userId, Type = type, Priority = priority, Title = title, Message = message, Channel = ReminderChannel.PushNotification, ScheduledAt = scheduledAt, RelatedEntityType = entityType, RelatedEntityId = entityId, Status = "Pending", SentStatus = false, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
    }

    private static ReminderResponseDto MapToDto(Reminder r) => new()
    {
        Id = r.Id, UserId = r.UserId, UserName = r.User?.FullName ?? "", Type = r.Type, Channel = r.Channel.ToString(),
        Priority = r.Priority, Title = r.Title, Message = r.Message, RelatedEntityType = r.RelatedEntityType,
        RelatedEntityId = r.RelatedEntityId, ScheduledAt = r.ScheduledAt, SentStatus = r.SentStatus, SentAt = r.SentAt,
        Status = r.Status, ReadAt = r.ReadAt, CompletedAt = r.CompletedAt, SnoozedUntil = r.SnoozedUntil,
        EscalationLevel = r.EscalationLevel, CreatedAt = r.CreatedAt,
    };
}
