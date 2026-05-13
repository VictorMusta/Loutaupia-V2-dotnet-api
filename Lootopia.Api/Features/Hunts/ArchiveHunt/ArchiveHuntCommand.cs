using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Hunts.ArchiveHunt;

public record ArchiveHuntCommand(Guid HuntId) : IRequest<Result>;
