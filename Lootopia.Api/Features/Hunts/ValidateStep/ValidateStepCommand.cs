using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ValidateStep;

public record ValidateStepCommand(Guid PlayerId, Guid HuntId, int StepOrder, double Latitude, double Longitude)
    : IRequest<Result<ValidateStepResponse>>;

public record ValidateStepResponse(
    bool Success,
    string Message,
    decimal? Reward,
    bool HuntCompleted,
    string? Clue,
    int? NextStepOrder,
    int TotalSteps);
