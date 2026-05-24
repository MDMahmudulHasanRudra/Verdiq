using Verdiq.Application.DTOs.Admin;

namespace Verdiq.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null);
    Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isActive);
    Task DeleteUserAsync(Guid userId);
    Task<List<AdminCaseDto>> GetCasesAsync();
    Task<List<AdminRevenueDto>> GetRevenueAsync(int months = 6);
    Task<AdminSystemStatsDto> GetSystemStatsAsync();
}
