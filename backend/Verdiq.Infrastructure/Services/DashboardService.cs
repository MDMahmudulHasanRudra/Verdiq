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

    public async Task<DashboardStats> GetDashboardStatsAsync(Guid lawyerId)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);
        var lastMonthStart = now.AddMonths(-1);

        var cases = _context.Cases.Where(c => c.AssignedLawyerId == lawyerId);

        var totalCases = await cases.CountAsync();
        var activeCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Active);
        var pendingCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Pending);
        var closedCases = await cases.CountAsync(c => c.Status == Domain.Enums.CaseStatus.Closed);

        var hearingsToday = await _context.Hearings
            .CountAsync(h => h.Case.AssignedLawyerId == lawyerId
                && h.HearingDate >= todayStart && h.HearingDate < todayEnd
                && h.Status == Domain.Enums.HearingStatus.Scheduled);

        var upcomingHearings = await _context.Hearings
            .CountAsync(h => h.Case.AssignedLawyerId == lawyerId
                && h.HearingDate >= now
                && h.Status == Domain.Enums.HearingStatus.Scheduled);

        var totalClients = await _context.Clients
            .CountAsync(c => c.AssignedLawyerId == lawyerId && c.IsActive);

        var unreadNotifications = await _context.Notifications
            .CountAsync(n => n.UserId == lawyerId && !n.IsRead);

        var lastMonthCases = await cases
            .CountAsync(c => c.CreatedAt >= lastMonthStart);

        var monthBeforeLast = await cases
            .CountAsync(c => c.CreatedAt >= lastMonthStart.AddMonths(-1)
                && c.CreatedAt < lastMonthStart);

        var caseGrowth = monthBeforeLast > 0
            ? ((double)(lastMonthCases - monthBeforeLast) / monthBeforeLast) * 100
            : 0;

        var lastMonthClients = await _context.Clients
            .CountAsync(c => c.AssignedLawyerId == lawyerId && c.CreatedAt >= lastMonthStart);

        var clientsMonthBefore = await _context.Clients
            .CountAsync(c => c.AssignedLawyerId == lawyerId
                && c.CreatedAt >= lastMonthStart.AddMonths(-1)
                && c.CreatedAt < lastMonthStart);

        var clientGrowth = clientsMonthBefore > 0
            ? ((double)(lastMonthClients - clientsMonthBefore) / clientsMonthBefore) * 100
            : 0;

        return new DashboardStats
        {
            TotalCases = totalCases,
            ActiveCases = activeCases,
            PendingCases = pendingCases,
            ClosedCases = closedCases,
            HearingsToday = hearingsToday,
            UpcomingHearings = upcomingHearings,
            TotalClients = totalClients,
            UnreadNotifications = unreadNotifications,
            CaseGrowth = Math.Round(caseGrowth, 1),
            ClientGrowth = Math.Round(clientGrowth, 1)
        };
    }

    public async Task<List<CaseChartDataPoint>> GetCaseChartDataAsync(Guid lawyerId, int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

        var cases = await _context.Cases
            .Where(c => c.AssignedLawyerId == lawyerId && !c.IsDeleted)
            .ToListAsync();

        var results = new List<CaseChartDataPoint>();

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

            results.Add(new CaseChartDataPoint
            {
                Month = monthLabel,
                Active = active,
                Closed = closed,
                Pending = pending,
            });
        }

        return results;
    }

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(Guid lawyerId, int count = 10)
    {
        var activities = new List<RecentActivity>();

        var recentCases = await _context.Cases
            .Where(c => c.AssignedLawyerId == lawyerId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .Select(c => new RecentActivity
            {
                Id = c.Id.ToString(),
                Type = "case",
                Title = $"Case filed: {c.CaseNumber}",
                Description = c.Title,
                Timestamp = c.CreatedAt.ToString("o"),
                ReferenceId = c.Id.ToString(),
            })
            .ToListAsync();

        activities.AddRange(recentCases);

        var recentHearings = await _context.Hearings
            .Where(h => h.Case.AssignedLawyerId == lawyerId && !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .Select(h => new RecentActivity
            {
                Id = h.Id.ToString(),
                Type = "hearing",
                Title = $"Hearing {h.HearingType}",
                Description = $"{h.Case.CaseNumber} - {h.Court}",
                Timestamp = h.CreatedAt.ToString("o"),
                ReferenceId = h.CaseId.ToString(),
            })
            .ToListAsync();

        activities.AddRange(recentHearings);

        var recentDocuments = await _context.Documents
            .Where(d => d.Case.AssignedLawyerId == lawyerId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .Select(d => new RecentActivity
            {
                Id = d.Id.ToString(),
                Type = "document",
                Title = $"Document uploaded: {d.OriginalFileName}",
                Description = d.Case.CaseNumber,
                Timestamp = d.CreatedAt.ToString("o"),
                ReferenceId = d.CaseId.ToString(),
            })
            .ToListAsync();

        activities.AddRange(recentDocuments);

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToList();
    }
}