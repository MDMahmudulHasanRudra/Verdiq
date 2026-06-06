using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Admin;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdminUserDto>> GetUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Chamber)
            .Include(u => u.AssignedCases.Where(c => !c.IsDeleted))
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                BarCouncilId = u.BarCouncilId,
                ChamberId = u.ChamberId,
                ChamberName = u.Chamber.Name,
                CasesCount = u.AssignedCases.Count(c => !c.IsDeleted),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return users;
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return (false, "User not found");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var status = user.IsActive ? "activated" : "deactivated";
        return (true, $"User {status} successfully");
    }

    public async Task<IEnumerable<AdminCaseDto>> GetCasesAsync()
    {
        return await _context.Cases
            .Include(c => c.ClientCases).ThenInclude(cc => cc.Client)
            .Include(c => c.AssignedLawyer)
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

    public async Task<IEnumerable<AdminRevenueDto>> GetRevenueAsync(int months = 6)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));
        var results = new List<AdminRevenueDto>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var label = monthStart.ToString("MMM yyyy");

            var payments = await _context.Payments
                .Where(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd && !p.IsDeleted)
                .ToListAsync();

            results.Add(new AdminRevenueDto
            {
                Period = label,
                Amount = payments.Sum(p => p.Amount),
                Transactions = payments.Count
            });
        }

        return results;
    }

    public async Task<AdminSystemStatsDto> GetSystemStatsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalChambers = await _context.Chambers.CountAsync(c => !c.IsDeleted);
        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
        var totalClients = await _context.Clients.CountAsync(c => !c.IsDeleted);
        var totalCases = await _context.Cases.CountAsync(c => !c.IsDeleted);
        var activeSubscriptions = await _context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted);

        var monthlyRevenue = await _context.Payments
            .Where(p => p.CreatedAt >= monthStart && !p.IsDeleted)
            .SumAsync(p => p.Amount);

        return new AdminSystemStatsDto
        {
            TotalChambers = totalChambers,
            TotalUsers = totalUsers,
            TotalCases = totalCases,
            TotalClients = totalClients,
            ActiveSubscriptions = activeSubscriptions,
            MonthlyRevenue = monthlyRevenue
        };
    }

    public async Task<AdminUserDto> CreateSubUserAsync(CreateSubUserDto dto, Guid currentUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new ArgumentException("Full name is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 4)
            throw new ArgumentException("Password must be at least 4 characters");
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role)
            || role == UserRole.SuperAdmin || role == UserRole.Owner || role == UserRole.Client)
            throw new ArgumentException("Invalid role. Valid: SeniorLawyer, JuniorLawyer, Assistant, Accountant");

        var currentUser = await _context.Users.FindAsync(currentUserId)
            ?? throw new UnauthorizedAccessException("Current user not found");

        var existingEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);
        if (existingEmail)
            throw new InvalidOperationException("A user with this email already exists");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            ChamberId = currentUser.ChamberId,
            IsActive = true,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = $"Admin created sub-user '{dto.FullName}' ({role})",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            BarCouncilId = user.BarCouncilId,
            ChamberId = user.ChamberId,
            ChamberName = currentUser.Chamber?.Name ?? "",
            CasesCount = 0,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new UserActivityDto
            {
                Id = a.Id,
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UserActivitySummaryDto>> GetUsersActivitySummaryAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activityGroups = await _context.AuditLogs
            .Where(a => a.CreatedAt >= monthStart)
            .GroupBy(a => a.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalActions = g.Count(),
                ActionsByModule = g.GroupBy(a => a.Entity)
                    .ToDictionary(sub => sub.Key, sub => sub.Count()),
                LastActivityAt = g.Max(a => a.CreatedAt)
            })
            .ToListAsync();

        var userIds = activityGroups.Select(g => g.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        return activityGroups.Select(g =>
        {
            users.TryGetValue(g.UserId, out var user);
            return new UserActivitySummaryDto
            {
                UserId = g.UserId,
                UserFullName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "",
                UserRole = user?.Role.ToString() ?? "",
                TotalActions = g.TotalActions,
                ActionsByModule = g.ActionsByModule,
                LastActivityAt = g.LastActivityAt
            };
        }).OrderByDescending(s => s.TotalActions);
    }

    public async Task<List<string>> GetUserModulesAsync(Guid userId)
    {
        return await _context.Set<UserModule>()
            .Where(m => m.UserId == userId)
            .Select(m => m.ModuleName)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task SetUserModulesAsync(Guid userId, List<string> modules)
    {
        var existing = await _context.Set<UserModule>()
            .Where(m => m.UserId == userId)
            .ToListAsync();

        _context.Set<UserModule>().RemoveRange(existing);

        foreach (var module in modules.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _context.Set<UserModule>().Add(new UserModule
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ModuleName = module,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
