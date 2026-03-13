namespace Lootopia.Api.Features.Auctions.CloseAuction;

public record CloseAuctionResponse(string Status, Guid? WinnerId, decimal? FinalPrice);
