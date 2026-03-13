namespace Lootopia.Api.Features.Auctions.ListAuctions;

public record ListAuctionsResponse(IReadOnlyList<AuctionSummary> Items, int TotalCount);

public record AuctionSummary(
    Guid Id,
    Guid SellerId,
    Guid ItemId,
    decimal ReservePrice,
    DateTime EndTime,
    string Status,
    decimal? CurrentHighestBid,
    string? ItemName,
    string? ItemImageUrl);
