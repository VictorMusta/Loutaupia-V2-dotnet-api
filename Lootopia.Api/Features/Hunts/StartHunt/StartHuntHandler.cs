using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.StartHunt;

public sealed class StartHuntHandler(LootopiaDbContext db) : IRequestHandler<StartHuntCommand, Result<StartHuntResponse>>
{
    public async Task<Result<StartHuntResponse>> Handle(StartHuntCommand request, CancellationToken cancellationToken)
    {
        var hunt = await db.Hunts
            .Include(h => h.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(h => h.Id == request.HuntId, cancellationToken);

        if (hunt is null)
            return Result.Failure<StartHuntResponse>(Error.NotFound);

        if (hunt.Status != HuntStatus.Active)
            return Result.Failure<StartHuntResponse>(Error.Custom("Hunt.NotActive", "Hunt is not active."));

        var inProgressHunt = await db.PlayerHunts
            .FirstOrDefaultAsync(
                ph => ph.PlayerId == request.PlayerId && ph.HuntId == request.HuntId && ph.Status == PlayerHuntStatus.InProgress,
                cancellationToken);

        if (inProgressHunt is not null)
            return Result.Failure<StartHuntResponse>(Error.Conflict);

        var firstStep = hunt.Steps.FirstOrDefault(s => s.StepOrder == 1);
        if (firstStep is null)
            return Result.Failure<StartHuntResponse>(Error.Custom("Hunt.NoSteps", "Hunt has no steps."));

        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = request.PlayerId,
            HuntId = request.HuntId,
            CurrentStepOrder = 1,
            Status = PlayerHuntStatus.InProgress
        };
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new StartHuntResponse(
            firstStep.Clue,
            firstStep.StepOrder,
            hunt.Steps.Count));
    }
}
