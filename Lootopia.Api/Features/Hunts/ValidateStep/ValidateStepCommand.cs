using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ValidateStep;

public record ValidateStepCommand(Guid PlayerId, Guid HuntId, int StepOrder, double Latitude, double Longitude)
    : IRequest<Result<ValidateStepResponse>>;

public record ValidateStepResponse(bool IsValid, string? Clue, int? NextStepOrder, int TotalSteps, bool HuntCompleted);
