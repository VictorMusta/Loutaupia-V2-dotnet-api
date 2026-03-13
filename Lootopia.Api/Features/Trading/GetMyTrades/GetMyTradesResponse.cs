namespace Lootopia.Api.Features.Trading.GetMyTrades;

public record GetMyTradesResponse(
    IReadOnlyList<TradeOfferDto> Offers,
    int TotalCount,
    int Page,
    int Size);

public record TradeOfferDto(
    Guid Id,
    Guid InitiatorId,
    Guid ReceiverId,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    IReadOnlyList<TradeOfferItemDto> OfferedItems,
    IReadOnlyList<TradeOfferItemDto> RequestedItems);

public record TradeOfferItemDto(Guid? ItemId, string? ItemName, int Quantity, decimal TokenAmount);
