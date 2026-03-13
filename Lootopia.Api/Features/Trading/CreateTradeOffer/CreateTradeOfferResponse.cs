namespace Lootopia.Api.Features.Trading.CreateTradeOffer;

public record CreateTradeOfferResponse(Guid OfferId, Guid ReceiverId, string Status, DateTime ExpiresAt, DateTime CreatedAt);
