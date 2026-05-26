using Verdiq.Application.DTOs.SuperAdmin;

namespace Verdiq.Application.Interfaces;

public interface ISuperAdminService
{
    Task<SuperAdminAuthResponse> LoginAsync(string userId, string password);
    Task<SuperAdminDashboardDto> GetDashboardAsync();
    Task<IEnumerable<ChamberManageDto>> GetAllChambersAsync();
    Task<ChamberManageDto> GetChamberDetailsAsync(Guid chamberId);
    Task<(bool Success, string Message)> UpdateChamberPlanAsync(Guid chamberId, string plan);
    Task<(bool Success, string Message, string? ImpersonationToken)> ImpersonateChamberAsync(Guid chamberId, Guid? userId);
    Task<ClearChamberResult> ClearChamberAsync(Guid chamberId);
    Task<IEnumerable<SuperAdminUserDto>> GetAllUsersAsync(Guid? chamberId = null);
    Task<(bool Success, string Message)> ResetUserPasswordAsync(Guid userId, string newPassword);
    Task<(bool Success, string Message)> ToggleUserStatusAsync(Guid userId);
    Task<SystemHealthDto> GetSystemHealthAsync();
}
