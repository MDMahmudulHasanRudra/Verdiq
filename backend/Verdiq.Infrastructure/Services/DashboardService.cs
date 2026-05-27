using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetStatsAsync(Guid chamberId)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);
        var lastMonthStart = now.AddMonths(-1);

        var cases = _context.Cases.Where(c => c.ChamberId == chamberId);

        var totalCases = await cases.CountAsync();
        var activeCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Active);
        var pendingCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Pending);
        var closedCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Closed);

        var hearingsToday = await _context.Hearings
            .CountAsync(h => h.Case.ChamberId == chamberId
                && h.HearingDate >= todayStart && h.HearingDate < todayEnd
                && h.Status == Domain.Enums.HearingStatus.Scheduled);

        var upcomingHearings = await _context.Hearings
            .CountAsync(h => h.Case.ChamberId == chamberId
                && h.HearingDate >= now
                && h.Status == Domain.Enums.HearingStatus.Scheduled);

        var totalClients = await _context.Clients
            .CountAsync(c => c.ChamberId == chamberId && c.IsActive);

        var totalLawyers = await _context.Users
            .CountAsync(u => u.ChamberId == chamberId && u.IsActive);

        var totalCasesLastMonth = await cases.CountAsync(c => c.CreatedAt >= lastMonthStart);
        var totalCasesPrevMonth = await cases.CountAsync(c =>
            c.CreatedAt >= lastMonthStart.AddMonths(-1) && c.CreatedAt < lastMonthStart);

        var caseGrowth = totalCasesPrevMonth > 0
            ? Math.Round((double)(totalCasesLastMonth - totalCasesPrevMonth) / totalCasesPrevMonth * 100, 1)
            : 0;

        var totalClientsLastMonth = await _context.Clients
            .CountAsync(c => c.ChamberId == chamberId && c.CreatedAt >= lastMonthStart);
        var totalClientsPrevMonth = await _context.Clients
            .CountAsync(c => c.ChamberId == chamberId
                && c.CreatedAt >= lastMonthStart.AddMonths(-1)
                && c.CreatedAt < lastMonthStart);

        var clientGrowth = totalClientsPrevMonth > 0
            ? Math.Round((double)(totalClientsLastMonth - totalClientsPrevMonth) / totalClientsPrevMonth * 100, 1)
            : 0;

        return new
        {
            TotalCases = totalCases,
            ActiveCases = activeCases,
            PendingCases = pendingCases,
            ClosedCases = closedCases,
            HearingsToday = hearingsToday,
            UpcomingHearings = upcomingHearings,
            TotalClients = totalClients,
            TotalLawyers = totalLawyers,
            CaseGrowth = caseGrowth,
            ClientGrowth = clientGrowth
        };
    }

    public async Task<object> GetCaseChartAsync(Guid chamberId, int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var cases = await _context.Cases
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted)
            .ToListAsync();

        var results = new List<object>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthStart.ToString("MMM");

            var casesInMonth = cases.Where(c => c.CreatedAt < monthEnd).ToList();

            var active = casesInMonth.Count(c => c.Status == Domain.Enums.CaseStatus.Active
                && (!c.ClosingDate.HasValue || c.ClosingDate >= monthStart));
            var closed = casesInMonth.Count(c => c.Status == Domain.Enums.CaseStatus.Closed
                || (c.ClosingDate.HasValue && c.ClosingDate >= monthStart && c.ClosingDate < monthEnd));
            var pending = casesInMonth.Count(c => c.Status == Domain.Enums.CaseStatus.Pending
                && (!c.ClosingDate.HasValue || c.ClosingDate >= monthStart));

            results.Add(new
            {
                Month = monthLabel,
                Active = active,
                Closed = closed,
                Pending = pending
            });
        }

        return results;
    }

    public async Task<object> GetRecentActivitiesAsync(Guid chamberId, int count = 10)
    {
        var recentCases = await _context.Cases
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .Select(c => new
            {
                Id = c.Id.ToString(),
                Type = "case",
                Title = $"Case filed: {c.CaseNumber}",
                Description = c.Title,
                Timestamp = c.CreatedAt.ToString("o"),
                ReferenceId = c.Id.ToString()
            })
            .ToListAsync();

        var recentHearings = await _context.Hearings
            .Where(h => h.Case.ChamberId == chamberId && !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .Select(h => new
            {
                Id = h.Id.ToString(),
                Type = "hearing",
                Title = $"Hearing - {h.Case.CaseNumber}",
                Description = $"{h.Case.CourtName} | {h.HearingDate:yyyy-MM-dd}",
                Timestamp = h.CreatedAt.ToString("o"),
                ReferenceId = h.CaseId.ToString()
            })
            .ToListAsync();

        var recentDocuments = await _context.Documents
            .Where(d => d.Case.ChamberId == chamberId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .Select(d => new
            {
                Id = d.Id.ToString(),
                Type = "document",
                Title = $"Document uploaded: {d.OriginalFileName}",
                Description = d.Case.CaseNumber,
                Timestamp = d.CreatedAt.ToString("o"),
                ReferenceId = d.CaseId.ToString()
            })
            .ToListAsync();

        var all = recentCases
            .Concat(recentHearings)
            .Concat(recentDocuments);

        return all
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToList();
    }

    public async Task<object> GetLawyerProductivityAsync(Guid chamberId)
    {
        var lawyers = await _context.Users
            .Where(u => u.ChamberId == chamberId && u.IsActive)
            .Select(u => new
            {
                Id = u.Id,
                Name = u.FullName,
                TotalCases = u.AssignedCases.Count(c => !c.IsDeleted),
                ActiveCases = u.AssignedCases.Count(c => c.Status == Domain.Enums.CaseStatus.Active && !c.IsDeleted),
                ClosedCases = u.AssignedCases.Count(c => c.Status == Domain.Enums.CaseStatus.Closed && !c.IsDeleted),
                PendingTasks = u.AssignedTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Pending)
            })
            .OrderByDescending(l => l.TotalCases)
            .ToListAsync();

        return lawyers;
    }

    public async Task<object> GetWinRatioAsync(Guid chamberId)
    {
        var lawyers = await _context.Users
            .Where(u => u.ChamberId == chamberId && u.IsActive)
            .Select(u => new
            {
                Id = u.Id,
                Name = u.FullName,
                TotalCases = u.AssignedCases.Count(c => !c.IsDeleted),
                ActiveCases = u.AssignedCases.Count(c => c.Status == Domain.Enums.CaseStatus.Active && !c.IsDeleted),
                PendingCases = u.AssignedCases.Count(c => c.Status == Domain.Enums.CaseStatus.Pending && !c.IsDeleted),
                ClosedCases = u.AssignedCases.Count(c => c.Status == Domain.Enums.CaseStatus.Closed && !c.IsDeleted)
            })
            .OrderByDescending(l => l.ClosedCases)
            .ToListAsync();

        return lawyers;
    }
}
