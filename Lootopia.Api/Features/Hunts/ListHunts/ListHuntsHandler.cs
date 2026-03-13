using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Geo;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Features.Hunts.ListHunts;

public sealed class ListHuntsHandler(
    LootopiaDbContext db,
    IGeoValidator geoValidator) : IRequestHandler<ListHuntsQuery, Result<ListHuntsResponse>>
{
    private const double MetersPerDegree = 111_320;

    public async Task<Result<ListHuntsResponse>> Handle(ListHuntsQuery request, CancellationToken cancellationToken)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var playerPoint = geometryFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));

        var radiusMeters = request.RadiusKm * 1000;
        var radiusDegrees = radiusMeters / MetersPerDegree;

        var hunts = await db.Hunts
            .AsNoTracking()
            .Include(h => h.Steps)
            .Where(h => h.Status == HuntStatus.Active)
            .Where(h => h.Steps.Any(s => s.Location.IsWithinDistance(playerPoint, radiusDegrees)))
            .Select(h => new
            {
                h.Id,
                h.Title,
                h.Description,
                h.Difficulty,
                StepCount = h.Steps.Count,
                h.RewardTokens,
                MinDistance = h.Steps.Min(s => s.Location.Distance(playerPoint))
            })
            .ToListAsync(cancellationToken);

        var nearestStepPerHunt = await db.HuntSteps
            .AsNoTracking()
            .Where(s => hunts.Select(h => h.Id).Contains(s.HuntId))
            .ToListAsync(cancellationToken);

        var items = hunts.Select(h =>
        {
            var nearestStep = nearestStepPerHunt.FirstOrDefault(s => s.HuntId == h.Id);
            var stepPoint = nearestStep?.Location;
            var distanceKm = stepPoint != null
                ? geoValidator.CalculateDistanceInMeters(playerPoint, stepPoint) / 1000
                : 0;
            return new HuntListItem(
                h.Id,
                h.Title,
                h.Description,
                h.Difficulty,
                h.StepCount,
                h.RewardTokens,
                Math.Round(distanceKm, 2));
        })
        .OrderBy(x => x.DistanceKm)
        .ToList();

        return Result.Success(new ListHuntsResponse(items));
    }
}
