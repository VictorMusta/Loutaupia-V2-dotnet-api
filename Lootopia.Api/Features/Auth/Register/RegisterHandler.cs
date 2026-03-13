using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using WalletEntity = Lootopia.Api.Domain.Entities.Wallet;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.Register;

public sealed class RegisterHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            return Result.Failure<RegisterResponse>(Error.Custom("Auth.EmailInUse", "Email is already in use."));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            Role = UserRole.Player,
            IsGuest = false,
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

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new RegisterResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
