using System.Security.Cryptography;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auth.MagicLink;

public sealed class GenerateMagicLinkHandler(LootopiaDbContext db) : IRequestHandler<GenerateMagicLinkCommand, Result<GenerateMagicLinkResponse>>
{
    private const int MagicLinkExpirationHours = 24;
    private const string BaseUrl = "https://app.lootopia.io/auth/magic"; // Configurable in production

    public async Task<Result<GenerateMagicLinkResponse>> Handle(GenerateMagicLinkCommand request, CancellationToken cancellationToken)
    {
        var adminUser = await db.Users.FindAsync([request.RequestingAdminId], cancellationToken);
        if (adminUser is null || adminUser.Role != UserRole.Admin)
            return Result.Failure<GenerateMagicLinkResponse>(Error.Forbidden);

        var partnerUser = await db.Users
            .Include(u => u.Partner)
            .FirstOrDefaultAsync(u => u.Id == request.PartnerUserId, cancellationToken);

        if (partnerUser is null)
            return Result.Failure<GenerateMagicLinkResponse>(Error.NotFound);

        if (partnerUser.Role != UserRole.Partner)
            return Result.Failure<GenerateMagicLinkResponse>(Error.Custom("MagicLink.InvalidUser", "Target user must be a partner."));

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var expiresAt = DateTime.UtcNow.AddHours(MagicLinkExpirationHours);

        var magicLink = new Domain.Entities.MagicLink
        {
            Id = Guid.NewGuid(),
            UserId = partnerUser.Id,
            Token = token,
            IsConsumed = false,
            ExpiresAt = expiresAt
        };
        db.MagicLinks.Add(magicLink);
        await db.SaveChangesAsync(cancellationToken);

        var magicLinkUrl = $"{BaseUrl}?token={token}";

        return Result.Success(new GenerateMagicLinkResponse(token, expiresAt, magicLinkUrl));
    }
}
