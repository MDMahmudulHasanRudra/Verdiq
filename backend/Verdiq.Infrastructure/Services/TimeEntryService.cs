using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.TimeEntry;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly AppDbContext _context;

    public TimeEntryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetAllAsync(Guid chamberId, string? status = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (from.HasValue)
            query = query.Where(t => t.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.StartTime <= to.Value);

        var entries = await query
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<TimeEntryResponseDto?> GetByIdAsync(Guid id, Guid chamberId)
    {
        var entry = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == id && t.ChamberId == chamberId && !t.IsDeleted);

        return entry == null ? null : MapToDto(entry);
    }

    public async Task<TimeEntryResponseDto> CreateAsync(CreateTimeEntryDto dto, Guid chamberId, Guid userId)
    {
        var now = DateTime.UtcNow;

        var entry = new TimeEntry
        {
            ChamberId = chamberId,
            UserId = userId,
            ClientId = dto.ClientId,
            CaseId = dto.CaseId,
            TaskId = dto.TaskId,
            Description = dto.Description,
            Category = dto.Category,
            HourlyRate = dto.HourlyRate,
            Billable = dto.Billable,
            Status = dto.Status,
            CreatedAt = now,
        };

        if (dto.StartTime.HasValue)
        {
            entry.StartTime = dto.StartTime.Value;
        }
        else
        {
            entry.StartTime = now;
        }

        if (dto.EndTime.HasValue)
        {
            entry.EndTime = dto.EndTime.Value;
        }

        if (dto.DurationMinutes.HasValue)
        {
            entry.DurationMinutes = dto.DurationMinutes.Value;
        }
        else if (dto.StartTime.HasValue && dto.EndTime.HasValue)
        {
            entry.DurationMinutes = (dto.EndTime.Value - dto.StartTime.Value).TotalMinutes;
        }

        // If completed/stopped, set end time and compute duration
        if (dto.Status == "Completed" && dto.EndTime == null)
        {
            entry.EndTime = now;
            entry.DurationMinutes = (now - entry.StartTime).TotalMinutes;
        }

        _context.Set<TimeEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // Reload with includes
        return (await GetByIdAsync(entry.Id, chamberId))!;
    }

    public async Task<TimeEntryResponseDto?> UpdateAsync(Guid id, UpdateTimeEntryDto dto, Guid chamberId)
    {
        var entry = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == id && t.ChamberId == chamberId && !t.IsDeleted);

        if (entry == null) return null;

        if (dto.ClientId.HasValue) entry.ClientId = dto.ClientId;
        if (dto.CaseId.HasValue) entry.CaseId = dto.CaseId;
        entry.TaskId = dto.TaskId ?? entry.TaskId;
        if (dto.Description != null) entry.Description = dto.Description;
        if (dto.Category != null) entry.Category = dto.Category;
        if (dto.StartTime.HasValue) entry.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) entry.EndTime = dto.EndTime.Value;
        if (dto.DurationMinutes.HasValue) entry.DurationMinutes = dto.DurationMinutes.Value;
        if (dto.HourlyRate.HasValue) entry.HourlyRate = dto.HourlyRate.Value;
        if (dto.Billable.HasValue) entry.Billable = dto.Billable.Value;
        if (dto.Status != null) entry.Status = dto.Status;

        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(entry);
    }

    public async Task<TimeEntryResponseDto?> UpdateStatusAsync(Guid id, UpdateTimeEntryStatusDto dto, Guid chamberId)
    {
        var entry = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == id && t.ChamberId == chamberId && !t.IsDeleted);

        if (entry == null) return null;

        var now = DateTime.UtcNow;
        entry.Status = dto.Status;
        entry.UpdatedAt = now;

        if (dto.Status == "Paused" && entry.EndTime == null)
        {
            entry.EndTime = now;
        }

        if (dto.Status == "Running" && entry.EndTime != null)
        {
            // Resuming - clear end time
            entry.EndTime = null;
        }

        if (dto.Status == "Completed")
        {
            entry.EndTime ??= now;
            entry.DurationMinutes = (entry.EndTime.Value - entry.StartTime).TotalMinutes;
        }

        await _context.SaveChangesAsync();
        return MapToDto(entry);
    }

    public async Task<TimeEntryResponseDto?> StopTimerAsync(Guid id, Guid chamberId)
    {
        return await UpdateStatusAsync(id, new UpdateTimeEntryStatusDto { Status = "Completed" }, chamberId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid chamberId)
    {
        var entry = await _context.Set<TimeEntry>()
            .FirstOrDefaultAsync(t => t.Id == id && t.ChamberId == chamberId && !t.IsDeleted);

        if (entry == null) return false;

        entry.IsDeleted = true;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TimeEntryResponseDto?> GetRunningTimerAsync(Guid userId)
    {
        var entry = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.UserId == userId && t.Status == "Running" && !t.IsDeleted)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefaultAsync();

        return entry == null ? null : MapToDto(entry);
    }

    public async Task<TimeSheetAnalyticsDto> GetAnalyticsAsync(Guid chamberId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted);

        if (from.HasValue)
            query = query.Where(t => t.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(t => t.StartTime <= to.Value);

        var entries = await query.ToListAsync();

        var totalBillable = entries.Where(e => e.Billable).Sum(e => e.DurationMinutes);
        var totalNonBillable = entries.Where(e => !e.Billable).Sum(e => e.DurationMinutes);
        var totalMinutes = entries.Sum(e => e.DurationMinutes);

        return new TimeSheetAnalyticsDto
        {
            TotalBillableHours = Math.Round(totalBillable / 60, 2),
            TotalNonBillableHours = Math.Round(totalNonBillable / 60, 2),
            TotalDurationMinutes = totalMinutes,
            TotalEntries = entries.Count,
            BillableEntries = entries.Count(e => e.Billable),
            NonBillableEntries = entries.Count(e => !e.Billable),
            UtilizationPercent = totalMinutes > 0
                ? Math.Round(totalBillable / totalMinutes * 100, 1)
                : 0,
            RevenueEstimate = entries.Where(e => e.Billable)
                .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
            ByLawyer = entries.GroupBy(e => new { e.UserId, e.User.FullName })
                .Select(g =>
                {
                    var lawyerBillable = g.Where(e => e.Billable).Sum(e => e.DurationMinutes);
                    var lawyerTotal = g.Sum(e => e.DurationMinutes);
                    return new TimeEntryByLawyer
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.FullName,
                        TotalHours = Math.Round(lawyerTotal / 60, 2),
                        BillableHours = Math.Round(lawyerBillable / 60, 2),
                        UtilizationPercent = lawyerTotal > 0
                            ? Math.Round(lawyerBillable / lawyerTotal * 100, 1)
                            : 0,
                        Revenue = g.Where(e => e.Billable)
                            .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                    };
                }).OrderByDescending(l => l.Revenue).ToList(),
            ByClient = entries.Where(e => e.ClientId != null && e.Client != null)
                .GroupBy(e => new { e.Client!.Name, Id = e.ClientId!.Value })
                .Select(g => new TimeEntryByClient
                {
                    ClientId = g.Key.Id,
                    ClientName = g.Key.Name,
                    TotalHours = Math.Round(g.Sum(e => e.DurationMinutes) / 60, 2),
                    Revenue = g.Where(e => e.Billable)
                        .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                }).OrderByDescending(c => c.Revenue).ToList(),
            ByCase = entries.Where(e => e.CaseId != null && e.Case != null)
                .GroupBy(e => new { e.Case!.Title, Id = e.CaseId!.Value })
                .Select(g => new TimeEntryByCase
                {
                    CaseId = g.Key.Id,
                    CaseTitle = g.Key.Title,
                    TotalHours = Math.Round(g.Sum(e => e.DurationMinutes) / 60, 2),
                    Revenue = g.Where(e => e.Billable)
                        .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                }).OrderByDescending(c => c.Revenue).ToList(),
            ByDay = entries.GroupBy(e => e.StartTime.Date)
                .Select(g =>
                {
                    var dayBillable = g.Where(e => e.Billable).Sum(e => e.DurationMinutes);
                    return new TimeEntryByDay
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        TotalHours = Math.Round(g.Sum(e => e.DurationMinutes) / 60, 2),
                        BillableHours = Math.Round(dayBillable / 60, 2),
                        Revenue = g.Where(e => e.Billable)
                            .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                    };
                }).OrderBy(d => d.Date).ToList(),
            ByCategory = entries.GroupBy(e => e.Category)
                .Select(g => new TimeEntryByCategory
                {
                    Category = g.Key,
                    TotalHours = Math.Round(g.Sum(e => e.DurationMinutes) / 60, 2),
                    Count = g.Count(),
                }).OrderByDescending(c => c.TotalHours).ToList(),
            MonthlyTrend = entries.GroupBy(e => new { e.StartTime.Year, e.StartTime.Month })
                .Select(g =>
                {
                    var monthBillable = g.Where(e => e.Billable).Sum(e => e.DurationMinutes);
                    return new MonthlyRevenueTrend
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Where(e => e.Billable)
                            .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                        BillableHours = Math.Round(monthBillable / 60, 2),
                    };
                }).OrderBy(m => m.Month).ToList(),
        };
    }

    public async Task<TeamCapacityDto> GetTeamCapacityAsync(Guid chamberId)
    {
        var members = await _context.Set<User>()
            .Where(u => u.ChamberId == chamberId && u.IsActive && !u.IsDeleted)
            .ToListAsync();

        var workDayHours = 8.0;
        var workDaysPerWeek = 5;
        var weeksPerPeriod = 4; // Monthly view

        var totalAvailable = members.Count * workDayHours * workDaysPerWeek * weeksPerPeriod;

        var from = DateTime.UtcNow.AddDays(-workDaysPerWeek * weeksPerPeriod);
        var entries = await _context.Set<TimeEntry>()
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted && t.StartTime >= from)
            .ToListAsync();

        var totalBooked = entries.Sum(e => e.DurationMinutes) / 60;

        return new TeamCapacityDto
        {
            TotalLawyers = members.Count,
            TotalAvailableHours = totalAvailable,
            TotalBookedHours = Math.Round(totalBooked, 2),
            UtilizationPercent = totalAvailable > 0
                ? Math.Round(totalBooked / totalAvailable * 100, 1)
                : 0,
            InactiveLawyers = members.Count(m => !entries.Any(e => e.UserId == m.Id)),
            Lawyers = members.Select(m =>
            {
                var memberEntries = entries.Where(e => e.UserId == m.Id).ToList();
                var booked = memberEntries.Sum(e => e.DurationMinutes) / 60;
                var avail = workDayHours * workDaysPerWeek * weeksPerPeriod;
                return new LawyerUtilization
                {
                    UserId = m.Id,
                    UserName = m.FullName,
                    AvailableHours = avail,
                    BookedHours = Math.Round(booked, 2),
                    UtilizationPercent = avail > 0
                        ? Math.Round(booked / avail * 100, 1)
                        : 0,
                    IsInactive = !memberEntries.Any(),
                    Revenue = memberEntries.Where(e => e.Billable)
                        .Sum(e => (decimal)e.DurationMinutes / 60 * e.HourlyRate),
                };
            }).OrderByDescending(l => l.BookedHours).ToList(),
        };
    }

    public async Task<TimeEntryResponseDto?> ApproveAsync(Guid id, Guid chamberId)
    {
        return await UpdateStatusAsync(id, new UpdateTimeEntryStatusDto { Status = "Completed" }, chamberId);
    }

    public async Task<TimeEntryResponseDto?> RejectAsync(Guid id, Guid chamberId)
    {
        var entry = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == id && t.ChamberId == chamberId && !t.IsDeleted);

        if (entry == null) return null;

        entry.Status = "Completed";
        entry.Billable = false;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(entry);
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (from.HasValue)
            query = query.Where(t => t.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(t => t.StartTime <= to.Value);

        var entries = await query
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetByCaseAsync(Guid caseId, Guid chamberId)
    {
        var entries = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.CaseId == caseId && t.ChamberId == chamberId && !t.IsDeleted)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetByInvoiceAsync(Guid invoiceId, Guid chamberId)
    {
        var entries = await _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.InvoiceId == invoiceId && t.ChamberId == chamberId && !t.IsDeleted)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<List<TimeEntryResponseDto>> GetUninvoicedAsync(Guid chamberId, Guid? clientId = null)
    {
        var query = _context.Set<TimeEntry>()
            .Include(t => t.User)
            .Include(t => t.Client)
            .Include(t => t.Case)
            .Include(t => t.Task)
            .Include(t => t.Invoice)
            .Where(t => t.ChamberId == chamberId && !t.IsDeleted
                && t.Billable && t.Status == "Completed" && t.InvoiceId == null);

        if (clientId.HasValue)
            query = query.Where(t => t.ClientId == clientId.Value);

        var entries = await query
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return entries.Select(MapToDto).ToList();
    }

    public async Task<bool> MarkAsInvoicedAsync(List<Guid> entryIds, Guid invoiceId, Guid chamberId)
    {
        var entries = await _context.Set<TimeEntry>()
            .Where(t => entryIds.Contains(t.Id) && t.ChamberId == chamberId && !t.IsDeleted)
            .ToListAsync();

        if (entries.Count == 0) return false;

        foreach (var entry in entries)
        {
            entry.InvoiceId = invoiceId;
            entry.Status = "Invoiced";
            entry.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static TimeEntryResponseDto MapToDto(TimeEntry t)
    {
        var totalAmount = t.Billable
            ? (decimal)t.DurationMinutes / 60 * t.HourlyRate
            : 0;

        return new TimeEntryResponseDto
        {
            Id = t.Id,
            UserId = t.UserId,
            UserName = t.User?.FullName ?? "",
            ClientId = t.ClientId,
            ClientName = t.Client?.Name,
            CaseId = t.CaseId,
            CaseTitle = t.Case?.Title,
            CaseNumber = t.Case?.CaseNumber,
            TaskId = t.TaskId,
            TaskTitle = t.Task?.Title,
            InvoiceId = t.InvoiceId,
            InvoiceNumber = t.Invoice?.InvoiceNumber,
            Description = t.Description,
            Category = t.Category,
            StartTime = t.StartTime,
            EndTime = t.EndTime,
            DurationMinutes = t.DurationMinutes,
            HourlyRate = t.HourlyRate,
            TotalAmount = totalAmount,
            Billable = t.Billable,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
        };
    }
}
