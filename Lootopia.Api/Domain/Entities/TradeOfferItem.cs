namespace Lootopia.Api.Domain.Entities;

public class TradeOfferItem
{
    public Guid Id { get; set; }
    public Guid TradeOfferId { get; set; }
    public string Side { get; set; } = "Offered"; // Offered, Requested
    public Guid? ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal TokenAmount { get; set; }

    public TradeOffer TradeOffer { get; set; } = null!;
    public Item? Item { get; set; }
}
