using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.StartHunt;

public record StartHuntCommand(Guid PlayerId, Guid HuntId) : IRequest<Result<StartHuntResponse>>;

public record StartHuntResponse(string Clue, int StepOrder, int TotalSteps);
