using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.GetHunt;

public sealed class GetHuntHandler(LootopiaDbContext db) : IRequestHandler<GetHuntQuery, Result<GetHuntResponse>>
{
    public async Task<Result<GetHuntResponse>> Handle(GetHuntQuery request, CancellationToken cancellationToken)
    {
        var hunt = await db.Hunts
            .AsNoTracking()
            .Include(h => h.Steps)
            .FirstOrDefaultAsync(h => h.Id == request.HuntId, cancellationToken);

        if (hunt is null)
            return Result.Failure<GetHuntResponse>(Error.NotFound);

        return Result.Success(new GetHuntResponse(
            hunt.Id,
            hunt.Title,
            hunt.Description,
            hunt.Status.ToString(),
            hunt.Difficulty,
            hunt.RewardTokens,
            hunt.Steps.Count));
    }
}
