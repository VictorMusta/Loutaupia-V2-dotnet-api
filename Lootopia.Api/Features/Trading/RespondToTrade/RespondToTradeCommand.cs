using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Trading.RespondToTrade;

public record RespondToTradeCommand(Guid OfferId, Guid UserId, string Action) : IRequest<Result>;
