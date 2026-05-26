using Verdiq.Application.DTOs.Permission;

namespace Verdiq.Application.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<RolePermissionResponseDto>> GetAllRolePermissionsAsync();
    Task<bool> HasPermissionAsync(string role, string permissionName);
}
