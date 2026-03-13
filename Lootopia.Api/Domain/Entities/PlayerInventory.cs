using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class PlayerInventory
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public AcquisitionSource Source { get; set; } = AcquisitionSource.Hunt;
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    public User Player { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
