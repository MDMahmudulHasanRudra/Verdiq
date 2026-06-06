using Verdiq.Application.DTOs.Admin;
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

    Task<IEnumerable<SubscriptionManageDto>> GetAllSubscriptionsAsync();
    Task<SubscriptionManageDto?> GetChamberSubscriptionAsync(Guid chamberId);
    Task<(bool Success, string Message)> UpdateUserSubscriptionAsync(Guid userId, UpdateUserSubscriptionDto dto);
    Task<IEnumerable<SuperAdminUserDto>> GetAllUsersDetailedAsync(Guid? chamberId = null);

    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<IEnumerable<RolePermissionsDto>> GetRolePermissionsAsync();
    Task<(bool Success, string Message)> AssignPermissionsToRoleAsync(string role, List<Guid> permissionIds);
    Task<(bool Success, string Message)> RemovePermissionFromRoleAsync(string role, Guid permissionId);

    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50);
    Task<BillingOverviewDto> GetBillingOverviewAsync();
    Task<ClearChamberResult> ClearChamberDataAsync(Guid chamberId);

    Task<(bool Success, string Message)> BroadcastNotificationAsync(BroadcastNotificationDto dto);
    Task<SystemConfigDto> GetSystemConfigAsync();
    Task<(bool Success, string Message)> UpdateSystemConfigAsync(SystemConfigDto dto);
    Task<(bool Success, string Message)> CreateChamberAsync(CreateChamberDto dto);
    Task<(bool Success, string Message)> UpdateChamberAsync(Guid chamberId, UpdateChamberDto dto);
    Task<(bool Success, string Message)> DeleteChamberAsync(Guid chamberId);

    Task<(bool Success, string Message)> CreateAdminUserAsync(CreateAdminUserDto dto);

    Task<IEnumerable<AdminCaseDto>> GetAllCasesAsync();
    Task<IEnumerable<object>> GetRevenueChartDataAsync(int months = 12);
    Task<IEnumerable<object>> GetChamberGrowthDataAsync(int months = 12);
}
