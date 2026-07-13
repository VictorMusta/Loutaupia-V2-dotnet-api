namespace Lootopia.Api.Features.Auctions.ListAuctions;

public record ListAuctionsResponse(IReadOnlyList<AuctionSummary> Items, int TotalCount);

public record AuctionSummary(
    Guid Id,
    Guid SellerId,
    Guid ItemId,
    decimal ReservePrice,
    decimal MinIncrement,
    DateTime EndTime,
    string Status,
    decimal? CurrentHighestBid,
    decimal CurrentPrice,
    int BidCount,
    string? ItemName,
    string? ItemRarity,
    string? ItemImageUrl,
    string? SellerName);
