using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Verdiq.Application.DTOs.SuperAdmin;
using Verdiq.Application.Interfaces;
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
        var monthStart = new DateTime(now.Year, now.Month, 1);

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

        foreach (var chamber in chambers)
        {
            chamber.UsersCount = await _context.Users.CountAsync(u => u.ChamberId == chamber.Id && !u.IsDeleted);
            chamber.CasesCount = await _context.Cases.CountAsync(c => c.ChamberId == chamber.Id && !c.IsDeleted);
            chamber.ClientsCount = await _context.Clients.CountAsync(c => c.ChamberId == chamber.Id && !c.IsDeleted);
            chamber.TotalRevenue = await _context.Invoices
                .Where(i => i.Client.ChamberId == chamber.Id && !i.IsDeleted)
                .SumAsync(i => i.Amount);
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.ChamberId == chamber.Id && !s.IsDeleted);
            if (sub != null)
                chamber.SubscriptionStatus = sub.Status.ToString();
        }

        var totalChambers = chambers.Count;
        var newChambersThisMonth = await _context.Chambers
            .CountAsync(c => c.CreatedAt >= monthStart && !c.IsDeleted);

        var monthlyRevenue = await _context.Invoices
            .Where(i => i.CreatedAt >= monthStart && !i.IsDeleted)
            .SumAsync(i => i.Amount);

        var alerts = new List<SystemAlert>();

        var expiredSubscriptions = await _context.Subscriptions
            .Where(s => s.CurrentPeriodEnd < now && s.Status == SubscriptionStatus.Active && !s.IsDeleted)
            .CountAsync();
        if (expiredSubscriptions > 0)
            alerts.Add(new SystemAlert
            {
                Type = "subscription_expired",
                Message = $"{expiredSubscriptions} subscription(s) have expired",
                Severity = "warning",
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
            NewChambersThisMonth = newChambersThisMonth,
            NewCasesThisMonth = await _context.Cases.CountAsync(c => c.CreatedAt >= monthStart && !c.IsDeleted),
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

        foreach (var chamber in chambers)
        {
            chamber.UsersCount = await _context.Users.CountAsync(u => u.ChamberId == chamber.Id && !u.IsDeleted);
            chamber.CasesCount = await _context.Cases.CountAsync(c => c.ChamberId == chamber.Id && !c.IsDeleted);
            chamber.ClientsCount = await _context.Clients.CountAsync(c => c.ChamberId == chamber.Id && !c.IsDeleted);
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
            TotalRevenue = await _context.Invoices
                .Where(i => i.Client.ChamberId == chamberId && !i.IsDeleted)
                .SumAsync(i => i.Amount)
        };

        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);
        if (sub != null)
            dto.SubscriptionStatus = sub.Status.ToString();

        return dto;
    }

    public async Task<(bool Success, string Message)> UpdateChamberPlanAsync(Guid chamberId, string plan)
    {
        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var newPlan))
            return (false, "Invalid plan. Valid: Free, Pro, Chamber");

        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return (false, "Chamber not found");

        chamber.SubscriptionPlan = newPlan;
        chamber.UpdatedAt = DateTime.UtcNow;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId && !s.IsDeleted);

        if (subscription != null)
        {
            subscription.Plan = newPlan;
            subscription.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Subscriptions.Add(new Domain.Entities.Subscription
            {
                ChamberId = chamberId,
                Plan = newPlan,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return (true, $"Chamber plan upgraded to {newPlan}");
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

        return (true, $"Impersonating {targetUser.FullName} in {chamber.Name}", token);
    }

    public async Task<ClearChamberResult> ClearChamberAsync(Guid chamberId)
    {
        var chamber = await _context.Chambers.FindAsync(chamberId);
        if (chamber == null || chamber.IsDeleted)
            return new ClearChamberResult { Success = false, Message = "Chamber not found" };

        var users = await _context.Users.Where(u => u.ChamberId == chamberId).ToListAsync();
        var cases = await _context.Cases.Where(c => c.ChamberId == chamberId).ToListAsync();
        var clients = await _context.Clients.Where(c => c.ChamberId == chamberId).ToListAsync();
        var documents = await _context.Documents
            .Where(d => d.Case.ChamberId == chamberId)
            .ToListAsync();
        var invoices = await _context.Invoices
            .Where(i => i.Client.ChamberId == chamberId)
            .ToListAsync();
        var expenses = await _context.Expenses.Where(e => e.ChamberId == chamberId).ToListAsync();
        var tasks = await _context.Tasks.Where(t => t.ChamberId == chamberId).ToListAsync();

        foreach (var u in users) u.IsDeleted = true;
        foreach (var c in cases) c.IsDeleted = true;
        foreach (var c in clients) c.IsDeleted = true;
        foreach (var d in documents) d.IsDeleted = true;
        foreach (var i in invoices) i.IsDeleted = true;
        foreach (var e in expenses) e.IsDeleted = true;
        foreach (var t in tasks) t.IsDeleted = true;

        chamber.IsDeleted = true;
        chamber.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ClearChamberResult
        {
            Success = true,
            Message = $"Chamber '{chamber.Name}' cleared successfully",
            UsersDeleted = users.Count,
            CasesDeleted = cases.Count,
            ClientsDeleted = clients.Count,
            DocumentsDeleted = documents.Count,
            InvoicesDeleted = invoices.Count
        };
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
        await _context.SaveChangesAsync();

        var status = user.IsActive ? "activated" : "deactivated";
        return (true, $"User {status} successfully");
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var alerts = new List<string>();

        var dbHealthy = true;
        try
        {
            await _context.Database.CanConnectAsync();
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

    private string GenerateImpersonationToken(Domain.Entities.User user, Domain.Entities.Chamber chamber)
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
