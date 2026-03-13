namespace Lootopia.Api.Features.Inventory.GetInventory;

public record GetInventoryResponse(
    IReadOnlyList<InventoryItemDto> Items,
    int TotalCount,
    int Page,
    int Size);

public record InventoryItemDto(
    Guid ItemId,
    string Name,
    string Description,
    string Rarity,
    string Type,
    string? ImageUrl,
    int Quantity,
    bool IsTradeable,
    DateTime AcquiredAt);
