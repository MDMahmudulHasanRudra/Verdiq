using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Verdiq.Application.DTOs.Admin;
using Verdiq.Application.DTOs.SuperAdmin;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class SuperAdminService : ISuperAdminService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private const string SuperAdminUserId = "rudra";
    private static readonly string SuperAdminPasswordHash = BCrypt.Net.BCrypt.HashPassword("rudra");

    private static readonly SuperAdminInfo SuperAdminInfo = new()
    {
        Id = "superadmin",
        Name = "Super Admin",
        UserId = SuperAdminUserId,
        Role = "SuperAdmin"
    };

    public SuperAdminService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<SuperAdminAuthResponse> LoginAsync(string userId, string password)
    {
        if (userId != SuperAdminUserId)
            return new SuperAdminAuthResponse { Success = false, Message = "Invalid credentials" };

        if (!BCrypt.Net.BCrypt.Verify(password, SuperAdminPasswordHash))
            return new SuperAdminAuthResponse { Success = false, Message = "Invalid credentials" };

        var token = GenerateSuperAdminToken();

        return new SuperAdminAuthResponse
        {
            Success = true,
            Message = "Super Admin login successful",
            AccessToken = token,
            RefreshToken = token,
            Admin = SuperAdminInfo
        };
    }

    public async Task<SuperAdminDashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var chambers = await _context.Chambers
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChamberManageDto
            {
                Id = c.Id,
                Name = c.Name,
                Logo = c.Logo,
                Address = c.Address,
                Phone = c.Phone,
                SubscriptionPlan = c.SubscriptionPlan.ToString(),
                CreatedAt = c.CreatedAt,
                IsActive = true
            })
            .ToListAsync();

        var chamberIds = chambers.Select(c => c.Id).ToList();

        var usersCountByChamber = await _context.Users
            .Where(u => chamberIds.Contains(u.ChamberId) && !u.IsDeleted)
            .GroupBy(u => u.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var casesCountByChamber = await _context.Cases
            .Where(c => chamberIds.Contains(c.ChamberId) && !c.IsDeleted)
            .GroupBy(c => c.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var clientsCountByChamber = await _context.Clients
            .Where(c => chamberIds.Contains(c.ChamberId) && !c.IsDeleted)
            .GroupBy(c => c.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var invoicesByChamber = await _context.Invoices
            .Where(i => i.Client != null && chamberIds.Contains(i.Client.ChamberId) && !i.IsDeleted)
            .GroupBy(i => i.Client.ChamberId)
            .Select(g => new { ChamberId = g.Key, Total = g.Sum(i => i.Amount), Count = g.Count() })
            .ToDictionaryAsync(g => g.ChamberId);

        foreach (var chamber in chambers)
        {
            chamber.UsersCount = usersCountByChamber.GetValueOrDefault(chamber.Id);
            chamber.CasesCount = casesCountByChamber.GetValueOrDefault(chamber.Id);
            chamber.ClientsCount = clientsCountByChamber.GetValueOrDefault(chamber.Id);
            chamber.TotalRevenue = invoicesByChamber.GetValueOrDefault(chamber.Id)?.Total ?? 0;
            chamber.InvoicesCount = invoicesByChamber.GetValueOrDefault(chamber.Id)?.Count ?? 0;

            var sub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.ChamberId == chamber.Id && !s.IsDeleted);
            if (sub != null)
            {
                chamber.SubscriptionStatus = sub.Status.ToString();
                chamber.SubscriptionPlan = sub.Plan.ToString();
            }
        }

        var totalChambers = chambers.Count;
        var newChambersThisMonth = await _context.Chambers
            .CountAsync(c => c.CreatedAt >= monthStart && !c.IsDeleted);

        var monthlyRevenue = await _context.Invoices
            .Where(i => i.CreatedAt >= monthStart && !i.IsDeleted)
            .SumAsync(i => i.Amount);

        var totalRevenueAllTime = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .SumAsync(i => i.Amount);

        var totalDocuments = await _context.Documents.CountAsync(d => !d.IsDeleted);
        var totalHearings = await _context.Hearings.CountAsync(h => !h.IsDeleted);
        var totalPayments = await _context.Payments.CountAsync(p => !p.IsDeleted);

        var newUsersThisMonth = await _context.Users
            .CountAsync(u => u.CreatedAt >= monthStart && !u.IsDeleted);

        var alerts = new List<SystemAlert>();

        var expiredSubscriptions = await _context.Subscriptions
            .CountAsync(s => s.CurrentPeriodEnd < now && s.Status == SubscriptionStatus.Active && !s.IsDeleted);

        if (expiredSubscriptions > 0)
            alerts.Add(new SystemAlert
            {
                Type = "subscription_expired",
                Message = $"{expiredSubscriptions} subscription(s) have expired",
                Severity = "warning",
                Timestamp = now
            });

        var inactiveChambers = await _context.Chambers
            .CountAsync(c => !c.IsDeleted && !c.Users.Any(u => u.IsActive));
        if (inactiveChambers > 0)
            alerts.Add(new SystemAlert
            {
                Type = "inactive_chambers",
                Message = $"{inactiveChambers} chamber(s) have no active users",
                Severity = "info",
                Timestamp = now
            });

        var trialExpiringSoon = await _context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Trial
                && s.CurrentPeriodEnd > now
                && s.CurrentPeriodEnd < now.AddDays(3)
                && !s.IsDeleted);
        if (trialExpiringSoon > 0)
            alerts.Add(new SystemAlert
            {
                Type = "trial_expiring",
                Message = $"{trialExpiringSoon} trial(s) expiring within 3 days",
                Severity = "info",
                Timestamp = now
            });

        return new SuperAdminDashboardDto
        {
            TotalChambers = totalChambers,
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
            TotalCases = await _context.Cases.CountAsync(c => !c.IsDeleted),
            TotalClients = await _context.Clients.CountAsync(c => !c.IsDeleted),
            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted),
            MonthlyRevenue = monthlyRevenue,
            TotalRevenueAllTime = totalRevenueAllTime,
            NewChambersThisMonth = newChambersThisMonth,
            NewCasesThisMonth = await _context.Cases.CountAsync(c => c.CreatedAt >= monthStart && !c.IsDeleted),
            NewUsersThisMonth = newUsersThisMonth,
            ExpiredSubscriptions = expiredSubscriptions,
            TotalDocuments = totalDocuments,
            TotalHearings = totalHearings,
            TotalPayments = totalPayments,
            Chambers = chambers,
            Alerts = alerts
        };
    }

    public async Task<IEnumerable<ChamberManageDto>> GetAllChambersAsync()
    {
        var chambers = await _context.Chambers
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChamberManageDto
            {
                Id = c.Id,
                Name = c.Name,
                Logo = c.Logo,
                Address = c.Address,
                Phone = c.Phone,
                SubscriptionPlan = c.SubscriptionPlan.ToString(),
                CreatedAt = c.CreatedAt,
                IsActive = true
            })
            .ToListAsync();

        var chamberIds = chambers.Select(c => c.Id).ToList();

        var usersCount = await _context.Users
            .Where(u => chamberIds.Contains(u.ChamberId) && !u.IsDeleted)
            .GroupBy(u => u.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var casesCount = await _context.Cases
            .Where(c => chamberIds.Contains(c.ChamberId) && !c.IsDeleted)
            .GroupBy(c => c.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var clientsCount = await _context.Clients
            .Where(c => chamberIds.Contains(c.ChamberId) && !c.IsDeleted)
            .GroupBy(c => c.ChamberId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        foreach (var chamber in chambers)
        {
            chamber.UsersCount = usersCount.GetValueOrDefault(chamber.Id);
            chamber.CasesCount = casesCount.GetValueOrDefault(chamber.Id);
            chamber.ClientsCount = clientsCount.GetValueOrDefault(chamber.Id);
        }

        return chambers;
    }

    public async Task<ChamberManageDto> GetChamberDetailsAsync(Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            throw new KeyNotFoundException("Chamber not found");

        var dto = new ChamberManageDto
        {
            Id = chamber.Id,
            Name = chamber.Name,
            Logo = chamber.Logo,
            Address = chamber.Address,
            Phone = chamber.Phone,
            SubscriptionPlan = chamber.SubscriptionPlan.ToString(),
            CreatedAt = chamber.CreatedAt,
            IsActive = true,
            UsersCount = await _context.Users.CountAsync(u => u.ChamberId == chamberId && !u.IsDeleted),
            CasesCount = await _context.Cases.CountAsync(c => c.ChamberId == chamberId && !c.IsDeleted),
            ClientsCount = await _context.Clients.CountAsync(c => c.ChamberId == chamberId && !c.IsDeleted),
            DocumentsCount = await _context.Documents.CountAsync(d => d.Case != null && d.Case.ChamberId == chamberId && !d.IsDeleted),
            HearingsCount = await _context.Hearings.CountAsync(h => h.Case != null && h.Case.ChamberId == chamberId && !h.IsDeleted),
            InvoicesCount = await _context.Invoices.CountAsync(i => i.Client != null && i.Client.ChamberId == chamberId && !i.IsDeleted),
            TotalRevenue = await _context.Invoices
                .Where(i => i.Client != null && i.Client.ChamberId == chamberId && !i.IsDeleted)
                .SumAsync(i => i.Amount)
        };

        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);
        if (sub != null)
        {
            dto.SubscriptionStatus = sub.Status.ToString();
            dto.SubscriptionPlan = sub.Plan.ToString();
        }

        return dto;
    }

    public async Task<(bool Success, string Message)> UpdateChamberPlanAsync(Guid chamberId, string plan)
    {
        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var newPlan))
            return (false, "Invalid plan. Valid: Free, Pro, Chamber");

        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found");

        var oldPlan = chamber.SubscriptionPlan.ToString();
        chamber.SubscriptionPlan = newPlan;
        chamber.UpdatedAt = DateTime.UtcNow;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        if (subscription != null)
        {
            subscription.Plan = newPlan;
            subscription.UpdatedAt = DateTime.UtcNow;
            if (newPlan == SubscriptionPlan.Free)
                subscription.Status = SubscriptionStatus.Active;
            if (newPlan != SubscriptionPlan.Free && subscription.Status == SubscriptionStatus.Trial)
                subscription.Status = SubscriptionStatus.Active;
        }
        else
        {
            _context.Subscriptions.Add(new Subscription
            {
                ChamberId = chamberId,
                Plan = newPlan,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Plan changed from {oldPlan} to {newPlan}",
            Entity = "Chamber",
            EntityId = chamberId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Chamber plan changed from {oldPlan} to {newPlan}");
    }

    public async Task<(bool Success, string Message, string? ImpersonationToken)> ImpersonateChamberAsync(
        Guid chamberId, Guid? userId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found", null);

        var targetUser = userId.HasValue
            ? await _context.Users.FindAsync(userId.Value)
            : await _context.Users.FirstOrDefaultAsync(u =>
                u.ChamberId == chamberId && u.Role == UserRole.Owner && !u.IsDeleted);

        if (targetUser == null || targetUser.IsDeleted)
            return (false, "Target user not found in chamber", null);

        var token = GenerateImpersonationToken(targetUser, chamber);

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"SuperAdmin impersonated {targetUser.FullName} in {chamber.Name}",
            Entity = "User",
            EntityId = targetUser.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return (true, $"Impersonating {targetUser.FullName} in {chamber.Name}", token);
    }

    public async Task<ClearChamberResult> ClearChamberAsync(Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return new ClearChamberResult { Success = false, Message = "Chamber not found" };

        return await ClearChamberDataAsync(chamberId);
    }

    public async Task<IEnumerable<SuperAdminUserDto>> GetAllUsersAsync(Guid? chamberId = null)
    {
        var query = _context.Users
            .Include(u => u.Chamber)
            .Where(u => !u.IsDeleted && u.Role != UserRole.SuperAdmin)
            .AsQueryable();

        if (chamberId.HasValue)
            query = query.Where(u => u.ChamberId == chamberId.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new SuperAdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                ChamberId = u.ChamberId,
                ChamberName = u.Chamber.Name,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SuperAdminUserDto>> GetAllUsersDetailedAsync(Guid? chamberId = null)
    {
        var query = _context.Users
            .Include(u => u.Chamber)
            .Include(u => u.Subscription)
            .Where(u => !u.IsDeleted && u.Role != UserRole.SuperAdmin)
            .AsQueryable();

        if (chamberId.HasValue)
            query = query.Where(u => u.ChamberId == chamberId.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new SuperAdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                ChamberId = u.ChamberId,
                ChamberName = u.Chamber.Name,
                CreatedAt = u.CreatedAt,
                SubscriptionPlan = u.Subscription != null ? u.Subscription.Plan.ToString() : null,
                SubscriptionStatus = u.Subscription != null ? u.Subscription.Status.ToString() : null,
                SubscriptionEnd = u.Subscription != null ? u.Subscription.CurrentPeriodEnd : null
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ResetUserPasswordAsync(Guid userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
            return (false, "Password must be at least 4 characters");

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted || user.Role == UserRole.SuperAdmin)
            return (false, "User not found or cannot be modified");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.LoginAttempts = 0;
        user.LockoutEnd = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Password reset for user {user.FullName}",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Password reset successfully for {user.FullName}");
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted || user.Role == UserRole.SuperAdmin)
            return (false, "User not found or cannot be modified");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"User {(user.IsActive ? "activated" : "deactivated")}: {user.FullName}",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var status = user.IsActive ? "activated" : "deactivated";
        return (true, $"User {status} successfully");
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var alerts = new List<string>();

        var dbHealthy = true;
        try
        {
            dbHealthy = await _context.Database.CanConnectAsync();
        }
        catch
        {
            dbHealthy = false;
            alerts.Add("Database connection issue detected");
        }

        var expiredCount = await _context.Subscriptions
            .CountAsync(s => s.CurrentPeriodEnd < now && s.Status == SubscriptionStatus.Active && !s.IsDeleted);

        if (expiredCount > 0)
            alerts.Add($"{expiredCount} active subscription(s) have expired");

        var inactiveChambers = await _context.Chambers
            .CountAsync(c => !c.IsDeleted && !c.Users.Any(u => u.IsActive));
        if (inactiveChambers > 0)
            alerts.Add($"{inactiveChambers} chamber(s) have no active users");

        var trialExpiring = await _context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Trial
                && s.CurrentPeriodEnd > now
                && s.CurrentPeriodEnd < now.AddDays(3)
                && !s.IsDeleted);
        if (trialExpiring > 0)
            alerts.Add($"{trialExpiring} trial subscription(s) expiring within 3 days");

        return new SystemHealthDto
        {
            Status = alerts.Count == 0 ? "Healthy" : "Degraded",
            DatabaseStatus = dbHealthy ? "Connected" : "Disconnected",
            TotalChambers = await _context.Chambers.CountAsync(c => !c.IsDeleted),
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.Role != UserRole.SuperAdmin),
            TotalCases = await _context.Cases.CountAsync(c => !c.IsDeleted),
            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted),
            MonthlyRevenue = await _context.Invoices
                .Where(i => i.CreatedAt >= monthStart && !i.IsDeleted)
                .SumAsync(i => i.Amount),
            ActiveAlerts = alerts,
            LastBackup = "N/A (Configure backup solution)"
        };
    }

    public async Task<IEnumerable<SubscriptionManageDto>> GetAllSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.Chamber)
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriptionManageDto
            {
                Id = s.Id,
                ChamberId = s.ChamberId,
                ChamberName = s.Chamber.Name,
                Plan = s.Plan.ToString(),
                Status = s.Status.ToString(),
                CurrentPeriodStart = s.CurrentPeriodStart,
                CurrentPeriodEnd = s.CurrentPeriodEnd,
                CancelAtPeriodEnd = s.CancelAtPeriodEnd,
                UserFullName = s.User != null ? s.User.FullName : null
            })
            .ToListAsync();
    }

    public async Task<SubscriptionManageDto?> GetChamberSubscriptionAsync(Guid chamberId)
    {
        var sub = await _context.Subscriptions
            .Include(s => s.Chamber)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        if (sub == null) return null;

        return new SubscriptionManageDto
        {
            Id = sub.Id,
            ChamberId = sub.ChamberId,
            ChamberName = sub.Chamber.Name,
            Plan = sub.Plan.ToString(),
            Status = sub.Status.ToString(),
            CurrentPeriodStart = sub.CurrentPeriodStart,
            CurrentPeriodEnd = sub.CurrentPeriodEnd,
            CancelAtPeriodEnd = sub.CancelAtPeriodEnd,
            UserFullName = sub.User?.FullName
        };
    }

    public async Task<(bool Success, string Message)> UpdateUserSubscriptionAsync(Guid userId, UpdateUserSubscriptionDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == UserRole.SuperAdmin)
            return (false, "User not found");

        var subscription = user.Subscription;
        if (subscription == null)
        {
            subscription = new Subscription
            {
                ChamberId = user.ChamberId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CurrentPeriodStart = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription);
        }

        if (!string.IsNullOrWhiteSpace(dto.Plan) && Enum.TryParse<SubscriptionPlan>(dto.Plan, true, out var plan))
            subscription.Plan = plan;

        if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<SubscriptionStatus>(dto.Status, true, out var status))
            subscription.Status = status;

        if (dto.CurrentPeriodEnd.HasValue)
            subscription.CurrentPeriodEnd = DateTime.SpecifyKind(dto.CurrentPeriodEnd.Value, DateTimeKind.Utc);

        subscription.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Subscription updated for user {user.FullName}: Plan={dto.Plan}, Status={dto.Status}",
            Entity = "Subscription",
            EntityId = subscription.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Subscription updated for {user.FullName}");
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Module = p.Module
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<RolePermissionsDto>> GetRolePermissionsAsync()
    {
        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .OrderBy(rp => rp.Role)
            .ToListAsync();

        var grouped = rolePermissions
            .GroupBy(rp => rp.Role)
            .Select(g => new RolePermissionsDto
            {
                Role = g.Key.ToString(),
                Permissions = g.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Module = rp.Permission.Module
                }).ToList()
            });

        return grouped;
    }

    public async Task<(bool Success, string Message)> AssignPermissionsToRoleAsync(string role, List<Guid> permissionIds)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            return (false, $"Invalid role: {role}");

        if (userRole == UserRole.SuperAdmin)
            return (false, "Cannot modify SuperAdmin permissions");

        var existing = await _context.RolePermissions
            .Where(rp => rp.Role == userRole)
            .ToListAsync();

        var existingIds = existing.Select(e => e.PermissionId).ToHashSet();
        var toAdd = permissionIds.Where(id => !existingIds.Contains(id)).ToList();

        foreach (var permId in toAdd)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                Role = userRole,
                PermissionId = permId
            });
        }

        var toRemove = existing.Where(e => !permissionIds.Contains(e.PermissionId)).ToList();
        _context.RolePermissions.RemoveRange(toRemove);

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Permissions updated for role {role}: {toAdd.Count} added, {toRemove.Count} removed",
            Entity = "RolePermission",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Permissions updated for role {role}");
    }

    public async Task<(bool Success, string Message)> RemovePermissionFromRoleAsync(string role, Guid permissionId)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            return (false, $"Invalid role: {role}");

        var rp = await _context.RolePermissions
            .FirstOrDefaultAsync(r => r.Role == userRole && r.PermissionId == permissionId);

        if (rp == null)
            return (false, "Permission assignment not found");

        _context.RolePermissions.Remove(rp);

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Permission {permissionId} removed from role {role}",
            Entity = "RolePermission",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, "Permission removed from role");
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId.ToString(),
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<BillingOverviewDto> GetBillingOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalInvoices = await _context.Invoices.CountAsync(i => !i.IsDeleted);
        var totalPayments = await _context.Payments.CountAsync(p => !p.IsDeleted);
        var totalRevenue = await _context.Invoices.SumAsync(i => (decimal?)i.Amount) ?? 0;
        var monthlyRevenue = await _context.Invoices
            .Where(i => i.CreatedAt >= monthStart && !i.IsDeleted)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;
        var pendingPayments = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Pending && !p.IsDeleted);
        var completedPayments = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Completed && !p.IsDeleted);
        var failedPayments = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Failed && !p.IsDeleted);
        var pendingAmount = await _context.Invoices
            .Where(i => i.Status == PaymentStatus.Pending && !i.IsDeleted)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var recentPayments = await _context.Payments
            .Include(p => p.Client)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.PaidAt)
            .Take(20)
            .Select(p => new RecentPaymentDto
            {
                Id = p.Id,
                InvoiceNumber = p.InvoiceNumber,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                ClientName = p.Client.Name,
                PaidAt = p.PaidAt
            })
            .ToListAsync();

        return new BillingOverviewDto
        {
            TotalInvoices = totalInvoices,
            TotalPayments = totalPayments,
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            PendingPayments = pendingPayments,
            CompletedPayments = completedPayments,
            FailedPayments = failedPayments,
            PendingAmount = pendingAmount,
            RecentPayments = recentPayments
        };
    }

    public async Task<ClearChamberResult> ClearChamberDataAsync(Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return new ClearChamberResult { Success = false, Message = "Chamber not found" };

        var users = await _context.Users.Where(u => u.ChamberId == chamberId).ToListAsync();
        var cases = await _context.Cases.Where(c => c.ChamberId == chamberId).ToListAsync();
        var clients = await _context.Clients.Where(c => c.ChamberId == chamberId).ToListAsync();
        var caseIds = cases.Select(c => c.Id).ToList();
        var documents = await _context.Documents
            .Where(d => d.Case != null && caseIds.Contains(d.Case.Id))
            .ToListAsync();
        var hearings = await _context.Hearings
            .Where(h => h.Case != null && caseIds.Contains(h.Case.Id))
            .ToListAsync();
        var invoices = await _context.Invoices
            .Where(i => i.Client != null && i.Client.ChamberId == chamberId)
            .ToListAsync();
        var expenses = await _context.Expenses.Where(e => e.ChamberId == chamberId).ToListAsync();
        var tasks = await _context.Tasks.Where(t => t.ChamberId == chamberId).ToListAsync();

        foreach (var u in users) u.IsDeleted = true;
        foreach (var c in cases) c.IsDeleted = true;
        foreach (var c in clients) c.IsDeleted = true;
        foreach (var d in documents) d.IsDeleted = true;
        foreach (var h in hearings) h.IsDeleted = true;
        foreach (var i in invoices) i.IsDeleted = true;
        foreach (var e in expenses) e.IsDeleted = true;
        foreach (var t in tasks) t.IsDeleted = true;

        var subscriptions = await _context.Subscriptions
            .Where(s => s.ChamberId == chamberId).ToListAsync();
        foreach (var s in subscriptions) s.IsDeleted = true;

        chamber.IsDeleted = true;
        chamber.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Chamber '{chamber.Name}' cleared with all data",
            Entity = "Chamber",
            EntityId = chamberId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ClearChamberResult
        {
            Success = true,
            Message = $"Chamber '{chamber.Name}' cleared successfully",
            UsersDeleted = users.Count,
            CasesDeleted = cases.Count,
            ClientsDeleted = clients.Count,
            DocumentsDeleted = documents.Count,
            InvoicesDeleted = invoices.Count,
            HearingsDeleted = hearings.Count,
            ExpensesDeleted = expenses.Count,
            TasksDeleted = tasks.Count
        };
    }

    public async Task<(bool Success, string Message)> BroadcastNotificationAsync(BroadcastNotificationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
            return (false, "Title and message are required");

        IQueryable<User> usersQuery = _context.Users.Where(u => !u.IsDeleted && u.IsActive);

        if (dto.TargetChamberId.HasValue)
            usersQuery = usersQuery.Where(u => u.ChamberId == dto.TargetChamberId.Value);

        var targetUsers = await usersQuery.ToListAsync();

        foreach (var user in targetUsers)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Broadcast notification '{dto.Title}' sent to {targetUsers.Count} users",
            Entity = "Notification",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Notification broadcast to {targetUsers.Count} users");
    }

    public async Task<SystemConfigDto> GetSystemConfigAsync()
    {
        return new SystemConfigDto
        {
            AllowSelfRegistration = true,
            MaintenanceMode = false,
            TrialDays = 14,
            MaxLoginAttempts = 5,
            RequireEmailVerification = false,
            EnableAiFeatures = true,
            DefaultCurrency = "BDT"
        };
    }

    public async Task<(bool Success, string Message)> UpdateSystemConfigAsync(SystemConfigDto dto)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Action = "System configuration updated",
            Entity = "SystemConfig",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return (true, "System configuration updated");
    }

    public async Task<(bool Success, string Message)> CreateChamberAsync(CreateChamberDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Chamber name is required");

        if (!Enum.TryParse<SubscriptionPlan>(dto.Plan, true, out var plan))
            plan = SubscriptionPlan.Free;

        var chamber = new Chamber
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            SubscriptionPlan = plan,
            CreatedAt = DateTime.UtcNow
        };

        _context.Chambers.Add(chamber);

        _context.Subscriptions.Add(new Subscription
        {
            ChamberId = chamber.Id,
            Plan = plan,
            Status = plan == SubscriptionPlan.Free ? SubscriptionStatus.Active : SubscriptionStatus.Trial,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow
        });

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Chamber '{dto.Name}' created with {dto.Plan} plan",
            Entity = "Chamber",
            EntityId = chamber.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Chamber '{dto.Name}' created successfully");
    }

    public async Task<(bool Success, string Message)> UpdateChamberAsync(Guid chamberId, UpdateChamberDto dto)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            chamber.Name = dto.Name;
        if (dto.Address != null)
            chamber.Address = dto.Address;
        if (dto.Phone != null)
            chamber.Phone = dto.Phone;

        chamber.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Chamber '{chamber.Name}' details updated",
            Entity = "Chamber",
            EntityId = chamberId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, "Chamber updated successfully");
    }

    public async Task<(bool Success, string Message)> DeleteChamberAsync(Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found");

        chamber.IsDeleted = true;
        chamber.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Chamber '{chamber.Name}' deleted",
            Entity = "Chamber",
            EntityId = chamberId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, $"Chamber '{chamber.Name}' deleted");
    }

    public async Task<IEnumerable<AdminCaseDto>> GetAllCasesAsync()
    {
        return await _context.Cases
            .Include(c => c.AssignedLawyer)
            .Include(c => c.Chamber)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AdminCaseDto
            {
                Id = c.Id,
                CaseNumber = c.CaseNumber,
                Title = c.Title,
                CaseType = c.CaseType,
                Status = c.Status.ToString(),
                CourtName = c.CourtName,
                AssignedLawyerName = c.AssignedLawyer.FullName,
                FilingDate = c.FilingDate,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetRevenueChartDataAsync(int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddMonths(-months);

        var revenueData = await _context.Invoices
            .Where(i => i.CreatedAt >= startDate && !i.IsDeleted)
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                revenue = g.Sum(i => i.Amount),
                count = g.Count()
            })
            .OrderBy(x => x.year).ThenBy(x => x.month)
            .ToListAsync();

        return revenueData;
    }

    public async Task<IEnumerable<object>> GetChamberGrowthDataAsync(int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddMonths(-months);

        var growthData = await _context.Chambers
            .Where(c => c.CreatedAt >= startDate && !c.IsDeleted)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                count = g.Count()
            })
            .OrderBy(x => x.year).ThenBy(x => x.month)
            .ToListAsync();

        return growthData;
    }

    private string GenerateSuperAdminToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "VerdiqSecretKey2024SuperSecureLongKey!@#$%^&*()"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "superadmin"),
            new Claim(ClaimTypes.Name, "Super Admin"),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
            new Claim("userId", SuperAdminUserId),
            new Claim("isSuperAdmin", "true")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Verdiq",
            audience: _configuration["Jwt:Audience"] ?? "VerdiqApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateImpersonationToken(User user, Chamber chamber)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "VerdiqSecretKey2024SuperSecureLongKey!@#$%^&*()"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("chamberId", user.ChamberId.ToString()),
            new Claim("chamberName", chamber.Name),
            new Claim("impersonatedBy", "SuperAdmin"),
            new Claim("isImpersonated", "true")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Verdiq",
            audience: _configuration["Jwt:Audience"] ?? "VerdiqApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
