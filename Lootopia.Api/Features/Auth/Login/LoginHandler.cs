using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.Login;

public sealed class LoginHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsGuest, cancellationToken);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return Result.Failure<LoginResponse>(Error.Custom("Auth.InvalidCredentials", "Invalid email or password."));

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(Error.Custom("Auth.AccountDisabled", "Account is disabled."));

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(Error.Custom("Auth.InvalidCredentials", "Invalid email or password."));

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
