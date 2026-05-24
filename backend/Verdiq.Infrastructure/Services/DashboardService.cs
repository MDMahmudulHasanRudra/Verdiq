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
}
