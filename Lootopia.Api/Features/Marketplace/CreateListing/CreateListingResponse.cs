namespace Lootopia.Api.Features.Marketplace.CreateListing;

public record CreateListingResponse(Guid ListingId, Guid ItemId, decimal Price, int Stock, DateTime CreatedAt);
