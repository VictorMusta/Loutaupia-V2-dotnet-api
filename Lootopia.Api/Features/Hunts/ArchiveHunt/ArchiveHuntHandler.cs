using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.ArchiveHunt;

public sealed class ArchiveHuntHandler(LootopiaDbContext db) : IRequestHandler<ArchiveHuntCommand, Result>
{
    public async Task<Result> Handle(ArchiveHuntCommand request, CancellationToken cancellationToken)
    {
        var hunt = await db.Hunts
            .FirstOrDefaultAsync(h => h.Id == request.HuntId, cancellationToken);

        if (hunt is null)
            return Result.Failure(Error.NotFound);

        hunt.Status = HuntStatus.Frozen; // Rend la chasse définitivement invisible aux yeux des utilisateurs lambdas
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
