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

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync(Guid lawyerId);
}
