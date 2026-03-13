namespace Lootopia.Api.Domain.Entities;

public class Listing
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = "Active"; // Active, Sold, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Seller { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
