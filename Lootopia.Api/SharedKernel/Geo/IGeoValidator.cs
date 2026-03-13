using NetTopologySuite.Geometries;

namespace Lootopia.Api.SharedKernel.Geo;

public interface IGeoValidator
{
    double CalculateDistanceInMeters(Point from, Point to);

    bool IsWithinRadius(Point userPosition, Point target, double radiusMeters);

    bool IsSpeedValid(Point previousPosition, DateTime previousTime, Point currentPosition, DateTime currentTime, double maxSpeedKmh = 100);
}
