namespace Lootopia.Api.Features.Marketplace.PurchaseItem;

public record PurchaseItemResponse(Guid ListingId, Guid ItemId, int Quantity, decimal TotalAmount, DateTime PurchasedAt);
