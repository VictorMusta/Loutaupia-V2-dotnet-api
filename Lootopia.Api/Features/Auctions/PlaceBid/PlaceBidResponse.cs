namespace Lootopia.Api.Features.Auctions.PlaceBid;

public record PlaceBidResponse(Guid BidId, decimal Amount, DateTime? NewEndTime);
