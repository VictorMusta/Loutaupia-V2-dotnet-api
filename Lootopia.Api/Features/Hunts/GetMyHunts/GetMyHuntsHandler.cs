using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Hunts.GetMyHunts;

public sealed class GetMyHuntsHandler(LootopiaDbContext db)
    : IRequestHandler<GetMyHuntsQuery, Result<GetMyHuntsResponse>>
{
    public async Task<Result<GetMyHuntsResponse>> Handle(
        GetMyHuntsQuery request, CancellationToken cancellationToken)
    {
        var playerHunts = await db.PlayerHunts
            .AsNoTracking()
            .Where(ph => ph.PlayerId == request.PlayerId)
            .Include(ph => ph.Hunt)
                .ThenInclude(h => h.Steps.OrderBy(s => s.StepOrder))
            .Include(ph => ph.StepValidations)
            .OrderByDescending(ph => ph.StartedAt)
            .ToListAsync(cancellationToken);

        var dtos = playerHunts.Select(ph =>
        {
            var validatedStepIds = ph.StepValidations
                .Where(sv => sv.IsValid)
                .Select(sv => sv.StepId)
                .ToHashSet();

            var steps = ph.Hunt.Steps.Select(s => new PlayerHuntStepDto(
                s.StepOrder,
                s.Clue,
                s.ActionType.ToString(),
                s.RadiusMeters,
                s.Location.Y,
                s.Location.X,
                validatedStepIds.Contains(s.Id)
            )).ToList();

            return new PlayerHuntDto(
                ph.HuntId,
                ph.Hunt.Title,
                ph.Status.ToString(),
                ph.CurrentStepOrder,
                ph.StartedAt,
                ph.CompletedAt,
                steps);
        }).ToList();

        return Result.Success(new GetMyHuntsResponse(dtos));
    }
}
