using Verdiq.Domain.Entities;

namespace Verdiq.Domain.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)> RegisterAsync(
        string fullName, string email, string password, string phone, string role);

    Task<(bool Success, string Message, User? User, string? AccessToken, string? RefreshToken)> LoginAsync(
        string email, string password);

    Task<(bool Success, string Message, string? AccessToken, string? RefreshToken)> RefreshTokenAsync(
        string accessToken, string refreshToken);

    Task<bool> LogoutAsync(Guid userId);
}
