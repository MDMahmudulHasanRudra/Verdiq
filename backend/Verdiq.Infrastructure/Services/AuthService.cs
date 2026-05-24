using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Auth;
using Verdiq.Application.Validators;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)>
        RegisterAsync(string fullName, string email, string password, string phone, string role)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
            return (false, "Email already registered", null, null, null);

        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            return (false, "Invalid role", null, null, null);

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Phone = phone,
            Role = userRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        var subscription = new Subscription
        {
            UserId = user.Id,
            Plan = SubscriptionPlan.Free,
            Status = SubscriptionStatus.Trial,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);

        await _context.SaveChangesAsync();

        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return (true, "Registration successful", user, accessToken, refreshToken);
    }

    public async Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)>
        LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Invalid email or password", null, null, null);

        if (!user.IsActive)
            return (false, "Account is deactivated. Contact admin.", null, null, null);

        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return (true, "Login successful", user, accessToken, refreshToken);
    }

    public async Task<(bool Success, string Message, string? AccessToken, string? RefreshToken)>
        RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var userId = _jwtService.ValidateToken(accessToken);
        if (userId == null)
            return (false, "Invalid access token", null, null);

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return (false, "Invalid or expired refresh token", null, null);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return (true, "Tokens refreshed", newAccessToken, newRefreshToken);
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();
        return true;
    }
}
