using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.UnfreezeUser;

public record UnfreezeUserCommand(Guid UserId) : IRequest<Result>;
