using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.UpgradeGuest;

public sealed class UpgradeGuestHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<UpgradeGuestCommand, Result<UpgradeGuestResponse>>
{
    public async Task<Result<UpgradeGuestResponse>> Handle(UpgradeGuestCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UpgradeGuestResponse>(Error.NotFound);

        if (!user.IsGuest)
            return Result.Failure<UpgradeGuestResponse>(Error.Custom("Auth.NotGuest", "User is already a registered player."));

        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);
        if (emailExists)
            return Result.Failure<UpgradeGuestResponse>(Error.Custom("Auth.EmailInUse", "Email is already in use."));

        user.Email = request.Email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.DisplayName = request.DisplayName;
        user.Role = UserRole.Player;
        user.IsGuest = false;

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user, user.DeviceId);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpgradeGuestResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
