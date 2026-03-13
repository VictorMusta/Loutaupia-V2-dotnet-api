namespace Lootopia.Api.Features.Marketplace.ListListings;

public record ListListingsResponse(
    IReadOnlyList<ListingDto> Listings,
    int TotalCount,
    int Page,
    int Size);

public record ListingDto(
    Guid Id,
    Guid SellerId,
    Guid ItemId,
    string ItemName,
    string ItemRarity,
    string ItemType,
    decimal Price,
    int Stock,
    DateTime CreatedAt);
