using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.ActivateHunt;

public sealed class ActivateHuntHandler(LootopiaDbContext db) : IRequestHandler<ActivateHuntCommand, Result>
{
    public async Task<Result> Handle(ActivateHuntCommand request, CancellationToken cancellationToken)
    {
        var hunt = await db.Hunts
            .Include(h => h.Steps)
            .FirstOrDefaultAsync(h => h.Id == request.HuntId, cancellationToken);

        if (hunt is null)
            return Result.Failure(Error.NotFound);

        if (hunt.Status != HuntStatus.Draft)
            return Result.Failure(Error.Custom("Hunt.NotDraft", "Only draft hunts can be activated."));

        if (hunt.Steps.Count < 1)
            return Result.Failure(Error.Custom("Hunt.NoSteps", "Hunt must have at least one step to be activated."));

        hunt.Status = HuntStatus.Active;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
