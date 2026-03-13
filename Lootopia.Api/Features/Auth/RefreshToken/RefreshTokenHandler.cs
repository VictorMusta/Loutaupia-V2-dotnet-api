using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiresAt != null &&
                u.RefreshTokenExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (user is null)
            return Result.Failure<RefreshTokenResponse>(Error.Custom("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));

        if (!user.IsActive)
            return Result.Failure<RefreshTokenResponse>(Error.Custom("Auth.AccountDisabled", "Account is disabled."));

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user, user.DeviceId);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
