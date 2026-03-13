using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Trading.CreateTradeOffer;

public record TradeItemDto(Guid? ItemId, int Quantity, decimal TokenAmount);

public record CreateTradeOfferCommand(
    Guid InitiatorId,
    Guid ReceiverId,
    IReadOnlyList<TradeItemDto> OfferedItems,
    IReadOnlyList<TradeItemDto> RequestedItems,
    DateTime ExpiresAt) : IRequest<Result<CreateTradeOfferResponse>>;
