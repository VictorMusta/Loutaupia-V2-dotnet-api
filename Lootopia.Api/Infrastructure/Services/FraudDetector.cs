using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class FraudDetector(LootopiaDbContext db) : IFraudDetector
{
    private const int AnomalyThresholdCount = 5;
    private const int AnomalyTimeWindowSeconds = 60;
    private const double SamePointRadiusMeters = 50;
    private const double DegreesPerMeter = 1.0 / 111320;

    public async Task<Result> CheckForAnomaliesAsync(
        Guid userId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var fromTime = timestamp.AddSeconds(-AnomalyTimeWindowSeconds);
        var point = new Point(longitude, latitude) { SRID = 4326 };
        var radiusDegrees = SamePointRadiusMeters * DegreesPerMeter;

        var recentValidations = await db.StepValidations
            .Include(sv => sv.PlayerHunt)
            .Where(sv => sv.PlayerHunt.PlayerId == userId)
            .Where(sv => sv.ValidatedAt >= fromTime && sv.ValidatedAt <= timestamp.AddSeconds(1))
            .Where(sv => sv.PlayerLocation != null)
            .ToListAsync(cancellationToken);

        var validationsAtSamePoint = recentValidations
            .Where(sv => sv.PlayerLocation!.Distance(point) <= radiusDegrees)
            .ToList();

        if (validationsAtSamePoint.Count >= AnomalyThresholdCount)
        {
            var fraudAlert = new FraudAlert
            {
                Id = Guid.NewGuid(),
                Type = "DuplicateValidation",
                Description = $"User {userId} performed {validationsAtSamePoint.Count} validations at same location within {AnomalyTimeWindowSeconds}s.",
                RelatedUserId = userId,
                Severity = "High",
                Status = FraudAlertStatus.New,
                CreatedAt = DateTime.UtcNow
            };
            db.FraudAlerts.Add(fraudAlert);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
