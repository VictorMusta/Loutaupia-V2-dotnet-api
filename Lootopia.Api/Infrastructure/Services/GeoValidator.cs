using Lootopia.Api.SharedKernel.Geo;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class GeoValidator : IGeoValidator
{
    private const double EarthRadiusMeters = 6_371_000;

    public double CalculateDistanceInMeters(Point from, Point to)
    {
        var dLat = ToRadians(to.Y - from.Y);
        var dLon = ToRadians(to.X - from.X);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(from.Y)) * Math.Cos(ToRadians(to.Y)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    public bool IsWithinRadius(Point userPosition, Point target, double radiusMeters)
    {
        var distance = CalculateDistanceInMeters(userPosition, target);
        return distance <= radiusMeters;
    }

    public bool IsSpeedValid(
        Point previousPosition, DateTime previousTime,
        Point currentPosition, DateTime currentTime,
        double maxSpeedKmh = 100)
    {
        var distance = CalculateDistanceInMeters(previousPosition, currentPosition);
        var timeDiff = (currentTime - previousTime).TotalHours;

        if (timeDiff <= 0) return false;

        var speedKmh = (distance / 1000) / timeDiff;
        return speedKmh <= maxSpeedKmh;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
