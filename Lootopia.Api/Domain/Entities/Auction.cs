namespace Lootopia.Api.Domain.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid ItemId { get; set; }
    public decimal ReservePrice { get; set; }
    public decimal MinIncrement { get; set; } = 1;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Active"; // Active, Closed, Cancelled, NoSale
    public Guid? HighestBidId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Seller { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public Bid? HighestBid { get; set; }
    public ICollection<Bid> Bids { get; set; } = [];
}
