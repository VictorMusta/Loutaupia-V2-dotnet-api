using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.MagicLink;

public sealed class ValidateMagicLinkHandler(
    LootopiaDbContext db,
    ITokenService tokenService) : IRequestHandler<ValidateMagicLinkCommand, Result<ValidateMagicLinkResponse>>
{
    public async Task<Result<ValidateMagicLinkResponse>> Handle(ValidateMagicLinkCommand request, CancellationToken cancellationToken)
    {
        var magicLink = await db.MagicLinks
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Token == request.Token, cancellationToken);

        if (magicLink is null)
            return Result.Failure<ValidateMagicLinkResponse>(Error.Custom("MagicLink.Invalid", "Invalid magic link."));

        if (magicLink.IsConsumed)
            return Result.Failure<ValidateMagicLinkResponse>(Error.Custom("MagicLink.AlreadyUsed", "Magic link has already been used."));

        if (magicLink.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<ValidateMagicLinkResponse>(Error.Custom("MagicLink.Expired", "Magic link has expired."));

        var user = magicLink.User;
        if (!user.IsActive)
            return Result.Failure<ValidateMagicLinkResponse>(Error.Custom("Auth.AccountDisabled", "Account is disabled."));

        magicLink.IsConsumed = true;

        var (accessToken, refreshToken, refreshTokenExpiresAt) = tokenService.GenerateTokens(user);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new ValidateMagicLinkResponse(accessToken, refreshToken, refreshTokenExpiresAt));
    }
}
