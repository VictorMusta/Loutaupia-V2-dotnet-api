using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.GetHunt;

public record GetHuntQuery(Guid HuntId) : IRequest<Result<GetHuntResponse>>;

public record GetHuntResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    int Difficulty,
    decimal RewardTokens,
    int StepCount);
