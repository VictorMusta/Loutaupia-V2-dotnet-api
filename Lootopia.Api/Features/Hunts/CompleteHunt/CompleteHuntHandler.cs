using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.CompleteHunt;

public sealed class CompleteHuntHandler(
    LootopiaDbContext db,
    IWalletService walletService) : IRequestHandler<CompleteHuntCommand, Result>
{
    public async Task<Result> Handle(CompleteHuntCommand request, CancellationToken cancellationToken)
    {
        var playerHunt = await db.PlayerHunts
            .Include(ph => ph.Hunt)
            .FirstOrDefaultAsync(ph => ph.Id == request.PlayerHuntId, cancellationToken);

        if (playerHunt is null)
            return Result.Failure(Error.NotFound);

        playerHunt.Status = PlayerHuntStatus.Completed;
        playerHunt.CompletedAt = DateTime.UtcNow;

        if (playerHunt.Hunt.RewardTokens > 0)
        {
            var creditResult = await walletService.CreditAsync(
                playerHunt.PlayerId,
                playerHunt.Hunt.RewardTokens,
                $"Hunt completed: {playerHunt.Hunt.Title}",
                $"hunt-complete-{playerHunt.Id}",
                cancellationToken);
            if (creditResult.IsFailure)
                return creditResult;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
