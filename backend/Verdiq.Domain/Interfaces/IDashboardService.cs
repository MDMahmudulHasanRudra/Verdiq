namespace Verdiq.Domain.Interfaces;

public interface IDashboardService
{
    Task<object> GetStatsAsync(Guid chamberId);
    Task<object> GetCaseChartAsync(Guid chamberId, int months);
    Task<object> GetRecentActivitiesAsync(Guid chamberId, int count);
    Task<object> GetLawyerProductivityAsync(Guid chamberId);
    Task<object> GetWinRatioAsync(Guid chamberId);
}
