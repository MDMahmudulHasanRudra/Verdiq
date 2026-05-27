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
}
