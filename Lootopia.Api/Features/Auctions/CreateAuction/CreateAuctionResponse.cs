namespace Lootopia.Api.Features.Auctions.CreateAuction;

public record CreateAuctionResponse(Guid AuctionId, DateTime StartTime, DateTime EndTime);
