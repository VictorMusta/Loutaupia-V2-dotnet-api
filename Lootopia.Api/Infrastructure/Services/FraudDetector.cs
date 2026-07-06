using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class FraudDetector(LootopiaDbContext db, IMemoryCache cache) : IFraudDetector
{
    // Vitesse max réaliste en m/s (~120 km/h en voiture)
    private const double MaxSpeedMetersPerSecond = 33.3;

    public async Task<Result> CheckForAnomaliesAsync(
        Guid userId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        // Lire les règles configurées par l'admin
        var thresholdCount    = await GetSettingAsync("Fraud_ThresholdCount",    5,  cancellationToken);
        var timeWindowSeconds = await GetSettingAsync("Fraud_TimeWindowSeconds", 60, cancellationToken);
        var radiusMeters      = await GetSettingAsync("Fraud_RadiusMeters",      50, cancellationToken);

        var fromTime = timestamp.AddSeconds(-timeWindowSeconds);

        // Récupérer toutes les validations réussies du joueur dans la fenêtre de temps
        var recentValidations = await db.StepValidations
            .Include(sv => sv.PlayerHunt)
            .Where(sv => sv.PlayerHunt.PlayerId == userId)
            .Where(sv => sv.ValidatedAt >= fromTime && sv.ValidatedAt <= timestamp.AddSeconds(5))
            .Where(sv => sv.IsValid)
            .OrderByDescending(sv => sv.ValidatedAt)
            .ToListAsync(cancellationToken);

        // --- Vérification 1 : RapidValidation ---
        // Trop de validations dans la fenêtre de temps ET dans le rayon configuré
        var currentPoint = new Point(longitude, latitude) { SRID = 4326 };

        var validationsInRadius = recentValidations
            .Where(sv => sv.PlayerLocation != null &&
                         HaversineDistance(latitude, longitude, sv.PlayerLocation.Y, sv.PlayerLocation.X) <= radiusMeters)
            .ToList();

        if (validationsInRadius.Count >= thresholdCount)
        {
            await CreateAlertIfNotDuplicate(
                userId,
                "RapidValidation",
                $"Le joueur {userId} a validé {validationsInRadius.Count} étapes en moins de {timeWindowSeconds}s dans un rayon de {radiusMeters}m (seuil : {thresholdCount}).",
                "Critical",
                timestamp,
                cancellationToken);
        }

        // --- Vérification 2 : SpeedAnomaly ---
        // Vitesse de déplacement physiquement impossible entre deux validations consécutives ?
        if (recentValidations.Count >= 2)
        {
            var previousValidation = recentValidations.Skip(1).FirstOrDefault();

            if (previousValidation?.PlayerLocation != null)
            {
                var distanceMeters = HaversineDistance(
                    latitude, longitude,
                    previousValidation.PlayerLocation.Y,
                    previousValidation.PlayerLocation.X);

                var timeDiffSeconds = (timestamp - previousValidation.ValidatedAt).TotalSeconds;

                if (timeDiffSeconds > 0 && distanceMeters > 100)
                {
                    var speedMps = distanceMeters / timeDiffSeconds;
                    if (speedMps > MaxSpeedMetersPerSecond)
                    {
                        var speedKmh = speedMps * 3.6;
                        await CreateAlertIfNotDuplicate(
                            userId,
                            "SpeedAnomaly",
                            $"Le joueur {userId} a parcouru {distanceMeters:F0}m en {timeDiffSeconds:F0}s ({speedKmh:F0} km/h). Déplacement physiquement impossible.",
                            "High",
                            timestamp,
                            cancellationToken);
                    }
                }
            }
        }

        return Result.Success();
    }

    private async Task CreateAlertIfNotDuplicate(
        Guid userId,
        string type,
        string description,
        string severity,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        // Anti-doublon : pas plus d'une alerte du même type pour le même joueur dans les 10 min
        var recentAlertExists = await db.FraudAlerts
            .AnyAsync(a =>
                a.RelatedUserId == userId &&
                a.Type == type &&
                a.CreatedAt >= timestamp.AddMinutes(-10),
                cancellationToken);

        if (recentAlertExists) return;

        db.FraudAlerts.Add(new FraudAlert
        {
            Id            = Guid.NewGuid(),
            Type          = type,
            Description   = description,
            RelatedUserId = userId,
            Severity      = severity,
            Status        = FraudAlertStatus.New,
            CreatedAt     = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Distance en mètres entre deux coordonnées GPS (formule de Haversine).
    /// </summary>
    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private async Task<int> GetSettingAsync(string key, int defaultValue, CancellationToken ct)
    {
        return await cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
            if (setting != null && int.TryParse(setting.Value, out var val))
                return val;
            return defaultValue;
        });
    }
}
