namespace Lootopia.Api.Features.Auctions.GetAuction;

public record BidDto(
    Guid Id,
    Guid BidderId,
    string? BidderName,
    decimal Amount,
    DateTime CreatedAt);

public record GetAuctionResponse(
    Guid Id,
    Guid SellerId,
    string? SellerName,
    Guid ItemId,
    decimal ReservePrice,
    decimal MinIncrement,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal? CurrentHighestBid,
    decimal CurrentPrice,
    decimal MinBid,
    int BidCount,
    string? ItemName,
    string? ItemImageUrl,
    IReadOnlyList<BidDto> Bids);
