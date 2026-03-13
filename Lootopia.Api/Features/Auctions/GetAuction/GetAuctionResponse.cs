namespace Lootopia.Api.Features.Auctions.GetAuction;

public record GetAuctionResponse(
    Guid Id,
    Guid SellerId,
    Guid ItemId,
    decimal ReservePrice,
    decimal MinIncrement,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal? CurrentHighestBid,
    int BidCount,
    string? ItemName,
    string? ItemImageUrl);
