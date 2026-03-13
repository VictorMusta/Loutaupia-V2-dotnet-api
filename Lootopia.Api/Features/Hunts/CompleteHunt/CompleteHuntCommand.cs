using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.CompleteHunt;

public record CompleteHuntCommand(Guid PlayerHuntId) : IRequest<Result>;
