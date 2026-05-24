using Verdiq.Domain.Entities;

namespace Verdiq.Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    string GenerateTempToken(User user);
}
