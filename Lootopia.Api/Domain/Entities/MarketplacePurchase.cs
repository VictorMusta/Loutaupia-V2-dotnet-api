namespace Lootopia.Api.Domain.Entities;

public class MarketplacePurchase
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Listing Listing { get; set; } = null!;
    public User Buyer { get; set; } = null!;
}
