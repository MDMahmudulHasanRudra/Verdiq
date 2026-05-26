using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Permission;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermissionResponseDto>> GetAllRolePermissionsAsync()
    {
        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .OrderBy(rp => rp.Role)
            .ToListAsync();

        var grouped = rolePermissions
            .GroupBy(rp => rp.Role)
            .Select(g => new RolePermissionResponseDto
            {
                Role = g.Key.ToString(),
                Permissions = g.Select(rp => new PermissionResponseDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Module = rp.Permission.Module
                }).ToList()
            });

        return grouped;
    }

    public async Task<bool> HasPermissionAsync(string role, string permissionName)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            return false;

        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .AnyAsync(rp => rp.Role == userRole && rp.Permission.Name == permissionName);
    }
}
