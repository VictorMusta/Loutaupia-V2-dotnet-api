using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.FreezeUser;

public record FreezeUserCommand(Guid UserId) : IRequest<Result>;
