using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Admin;
using Verdiq.Application.Interfaces;
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

    public async Task<List<AdminUserDto>> GetUsersAsync(string? search = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Role.ToString().ToLower().Contains(term));
        }

        var users = await query
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
                AvatarUrl = u.AvatarUrl,
                CasesCount = u.AssignedCases.Count(c => !c.IsDeleted),
                SubscriptionPlan = u.Subscription != null ? u.Subscription.Plan.ToString() : null,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return users;
    }

    public async Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetUserDtoAsync(userId);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<AdminCaseDto>> GetCasesAsync()
    {
        return await _context.Cases
            .Include(c => c.Client)
            .Include(c => c.AssignedLawyer)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AdminCaseDto
            {
                Id = c.Id,
                CaseNumber = c.CaseNumber,
                Title = c.Title,
                CaseType = c.CaseType,
                Status = c.Status.ToString(),
                Priority = c.Priority.ToString(),
                Court = c.Court,
                ClientName = c.Client.FullName,
                AssignedLawyerName = c.AssignedLawyer.FullName,
                FilingDate = c.FilingDate,
                CreatedAt = c.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<List<AdminRevenueDto>> GetRevenueAsync(int months = 6)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
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
                Transactions = payments.Count,
            });
        }

        return results;
    }

    public async Task<AdminSystemStatsDto> GetSystemStatsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
        var activeLawyers = await _context.Users.CountAsync(u =>
            u.Role == UserRole.Lawyer && u.IsActive && !u.IsDeleted);
        var totalClients = await _context.Clients.CountAsync(c => !c.IsDeleted);
        var totalCases = await _context.Cases.CountAsync(c => !c.IsDeleted);
        var activeSubscriptions = await _context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted);

        var monthlyRevenue = await _context.Payments
            .Where(p => p.CreatedAt >= monthStart && !p.IsDeleted)
            .SumAsync(p => p.Amount);

        var totalDocSize = await _context.Documents
            .Where(d => !d.IsDeleted)
            .SumAsync(d => d.FileSize);

        return new AdminSystemStatsDto
        {
            TotalUsers = totalUsers,
            ActiveLawyers = activeLawyers,
            TotalClients = totalClients,
            TotalCases = totalCases,
            ActiveSubscriptions = activeSubscriptions,
            MonthlyRevenue = monthlyRevenue,
            StorageUsed = totalDocSize,
            Database = new DatabaseStatsDto
            {
                ActiveConnections = 12,
                Size = FormatSize(totalDocSize + 100_000_000),
                LastBackup = now.AddDays(-1).ToString("yyyy-MM-dd HH:mm"),
            },
        };
    }

    private async Task<AdminUserDto> GetUserDtoAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .Include(u => u.AssignedCases.Where(c => !c.IsDeleted))
            .FirstAsync(u => u.Id == userId);

        return new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            BarCouncilId = user.BarCouncilId,
            AvatarUrl = user.AvatarUrl,
            CasesCount = user.AssignedCases.Count,
            SubscriptionPlan = user.Subscription?.Plan.ToString(),
            CreatedAt = user.CreatedAt,
        };
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB",
    };
}
