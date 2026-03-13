using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using WalletEntity = Lootopia.Api.Domain.Entities.Wallet;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.GuestLogin;

public sealed class GuestLoginHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<GuestLoginCommand, Result<GuestLoginResponse>>
{
    public async Task<Result<GuestLoginResponse>> Handle(GuestLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.DeviceId == request.DeviceId && u.IsGuest, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                DeviceId = request.DeviceId,
                DisplayName = $"Guest_{request.DeviceId[..Math.Min(8, request.DeviceId.Length)]}",
                Role = UserRole.Guest,
                IsGuest = true,
                IsActive = true
            };
            db.Users.Add(user);

            var wallet = new WalletEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 0
            };
            db.Wallets.Add(wallet);
        }

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user, request.DeviceId);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new GuestLoginResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
