using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ListAllHunts;

public record ListAllHuntsQuery : IRequest<Result<ListAllHuntsResponse>>;

public record ListAllHuntsResponse(IReadOnlyList<AdminHuntItem> Hunts);

public record AdminHuntItem(
    Guid Id,
    string Title,
    string Description,
    int Difficulty,
    int StepCount,
    decimal RewardTokens,
    string Status,
    DateTime? StartDate,
    Guid CreatedBy);
