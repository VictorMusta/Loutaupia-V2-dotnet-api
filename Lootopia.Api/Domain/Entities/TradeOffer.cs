namespace Lootopia.Api.Domain.Entities;

public class TradeOffer
{
    public Guid Id { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Refused, Expired, CounterOffer
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Initiator { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public ICollection<TradeOfferItem> Items { get; set; } = [];
}
