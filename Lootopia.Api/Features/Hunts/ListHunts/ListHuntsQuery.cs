using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ListHunts;

public record ListHuntsQuery(double Lat, double Lng, double RadiusKm) : IRequest<Result<ListHuntsResponse>>;

public record ListHuntsResponse(IReadOnlyList<HuntListItem> Hunts);

public record HuntListItem(
    Guid Id,
    string Title,
    string Description,
    int Difficulty,
    int StepCount,
    decimal RewardTokens,
    double DistanceKm,
    double Latitude,
    double Longitude,
    string Status);
