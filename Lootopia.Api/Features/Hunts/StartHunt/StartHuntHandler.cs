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
        var playerExists = await db.Users.AnyAsync(u => u.Id == request.PlayerId, cancellationToken);
        if (!playerExists)
            return Result.Failure<StartHuntResponse>(Error.Custom("Auth.UserNotFound", "Player not found. Please log in again."));

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
        {
            var curStep = hunt.Steps.FirstOrDefault(s => s.StepOrder == inProgressHunt.CurrentStepOrder);
            return Result.Success(new StartHuntResponse(
                curStep?.Clue ?? string.Empty,
                inProgressHunt.CurrentStepOrder,
                hunt.Steps.Count));
        }

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
