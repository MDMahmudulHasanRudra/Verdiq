using Verdiq.Application.DTOs.Admin;

namespace Verdiq.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetUsersAsync();
    Task<(bool Success, string Message)> ToggleUserStatusAsync(Guid userId);
    Task<IEnumerable<AdminCaseDto>> GetCasesAsync();
    Task<IEnumerable<AdminRevenueDto>> GetRevenueAsync(int months);
    Task<AdminSystemStatsDto> GetSystemStatsAsync();
}
