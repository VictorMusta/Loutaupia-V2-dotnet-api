using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.ListAllHunts;

public sealed class ListAllHuntsHandler(LootopiaDbContext db)
    : IRequestHandler<ListAllHuntsQuery, Result<ListAllHuntsResponse>>
{
    public async Task<Result<ListAllHuntsResponse>> Handle(
        ListAllHuntsQuery request, CancellationToken cancellationToken)
    {
        var hunts = await db.Hunts
            .AsNoTracking()
            .Include(h => h.Steps)
            .OrderByDescending(h => h.StartDate ?? DateTime.MinValue)
            .ThenBy(h => h.Title)
            .Select(h => new AdminHuntItem(
                h.Id,
                h.Title,
                h.Description,
                h.Difficulty,
                h.Steps.Count,
                h.RewardTokens,
                h.Status.ToString(),
                h.StartDate,
                h.CreatedBy))
            .ToListAsync(cancellationToken);

        return Result.Success(new ListAllHuntsResponse(hunts));
    }
}
