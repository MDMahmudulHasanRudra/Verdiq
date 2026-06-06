using Verdiq.Application.DTOs.Admin;

namespace Verdiq.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetUsersAsync();
    Task<(bool Success, string Message)> ToggleUserStatusAsync(Guid userId);
    Task<IEnumerable<AdminCaseDto>> GetCasesAsync();
    Task<IEnumerable<AdminRevenueDto>> GetRevenueAsync(int months);
    Task<AdminSystemStatsDto> GetSystemStatsAsync();

    Task<AdminUserDto> CreateSubUserAsync(CreateSubUserDto dto, Guid currentUserId);
    Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<IEnumerable<UserActivitySummaryDto>> GetUsersActivitySummaryAsync();

    Task<List<string>> GetUserModulesAsync(Guid userId);
    Task SetUserModulesAsync(Guid userId, List<string> modules);
}
