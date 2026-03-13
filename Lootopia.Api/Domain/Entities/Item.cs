using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public ItemType Type { get; set; } = ItemType.Artifact;
    public string? ImageUrl { get; set; }
    public bool IsTradeable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PlayerInventory> PlayerInventories { get; set; } = [];
    public ICollection<Listing> Listings { get; set; } = [];
}
