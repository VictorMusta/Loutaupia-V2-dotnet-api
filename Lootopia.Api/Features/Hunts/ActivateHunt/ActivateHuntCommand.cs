using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ActivateHunt;

public record ActivateHuntCommand(Guid HuntId) : IRequest<Result>;
