using Lootopia.Api.Domain.Enums;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.CreateHunt;

public record CreateHuntCommand(
    Guid CreatedBy,
    string Title,
    string Description,
    int Difficulty,
    decimal RewardTokens,
    int MaxWinners,
    Guid? RewardItemId,
    IReadOnlyList<CreateHuntStepDto> Steps) : IRequest<Result<CreateHuntResponse>>;

public record CreateHuntStepDto(
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string Clue,
    StepActionType ActionType);

public record CreateHuntResponse(Guid HuntId);
