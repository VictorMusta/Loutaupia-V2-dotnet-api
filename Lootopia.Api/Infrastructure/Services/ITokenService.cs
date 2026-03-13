using Lootopia.Api.Domain.Entities;

namespace Lootopia.Api.Infrastructure.Services;

public interface ITokenService
{
    (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt) GenerateTokens(
        User user,
        string? deviceId = null);

    string GenerateAccessToken(User user, string? deviceId = null);
}
