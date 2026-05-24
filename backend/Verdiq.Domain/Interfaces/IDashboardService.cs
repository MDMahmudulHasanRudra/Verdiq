namespace Verdiq.Domain.Interfaces;

public class DashboardStats
{
    public int TotalCases { get; set; }
    public int ActiveCases { get; set; }
    public int PendingCases { get; set; }
    public int ClosedCases { get; set; }
    public int HearingsToday { get; set; }
    public int UpcomingHearings { get; set; }
    public int TotalClients { get; set; }
    public int UnreadNotifications { get; set; }
    public double CaseGrowth { get; set; }
    public double ClientGrowth { get; set; }
}

public class CaseChartDataPoint
{
    public string Month { get; set; } = string.Empty;
    public int Active { get; set; }
    public int Closed { get; set; }
    public int Pending { get; set; }
}

public class RecentActivity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
}

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync(Guid lawyerId);
    Task<List<CaseChartDataPoint>> GetCaseChartDataAsync(Guid lawyerId, int months = 12);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(Guid lawyerId, int count = 10);
}
