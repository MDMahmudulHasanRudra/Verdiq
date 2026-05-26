using Verdiq.Domain.Entities;

namespace Verdiq.Domain.Interfaces;

public interface IJwtService
{
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string GenerateTempToken(User user);
    Guid? ValidateToken(string token);
}
