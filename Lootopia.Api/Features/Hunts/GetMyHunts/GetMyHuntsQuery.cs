using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.GetMyHunts;

public record GetMyHuntsQuery(Guid PlayerId) : IRequest<Result<GetMyHuntsResponse>>;

public record GetMyHuntsResponse(IReadOnlyList<PlayerHuntDto> Hunts);

public record PlayerHuntDto(
    Guid HuntId,
    string HuntTitle,
    string Status,
    int CurrentStep,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<PlayerHuntStepDto> Steps);

public record PlayerHuntStepDto(
    int Order,
    string Clue,
    string ActionType,
    double RadiusMeters,
    double Latitude,
    double Longitude,
    bool Validated);
