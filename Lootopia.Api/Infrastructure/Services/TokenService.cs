using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "Lootopia.Api";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "Lootopia.Client";
    private readonly int _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
    private readonly int _refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

    public (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt) GenerateTokens(
        User user,
        string? deviceId = null)
    {
        var accessToken = GenerateAccessToken(user, deviceId);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);
        return (accessToken, refreshToken, refreshTokenExpiresAt);
    }

    public string GenerateAccessToken(User user, string? deviceId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        if (!string.IsNullOrEmpty(deviceId))
            claims.Add(new Claim("deviceId", deviceId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
