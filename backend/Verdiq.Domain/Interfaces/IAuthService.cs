using Verdiq.Domain.Entities;

namespace Verdiq.Domain.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)>
        RegisterAsync(string fullName, string email, string password, string phone, string role, Guid chamberId);

    Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)>
        LoginAsync(string email, string password);

    Task<(bool Success, string Message, string? AccessToken, string? RefreshToken)>
        RefreshTokenAsync(string accessToken, string refreshToken);

    Task<bool> LogoutAsync(Guid userId);

    Task<(bool Success, string Message, User? User)> UpdateProfileAsync(
        Guid userId, string fullName, string phone, string? barCouncilId);

    Task<(bool Success, string Message)> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword);
}
