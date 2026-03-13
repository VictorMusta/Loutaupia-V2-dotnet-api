namespace Lootopia.Api.Domain.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Auction Auction { get; set; } = null!;
    public User Bidder { get; set; } = null!;
}
