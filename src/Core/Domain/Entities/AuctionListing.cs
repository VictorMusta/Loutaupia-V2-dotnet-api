using System;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class AuctionListing
{
    private decimal _startingPrice;
    private decimal? _buyoutPrice;

    public Guid ListingId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid ArtefactId { get; private set; }
    public int Quantity { get; set; }

    public decimal StartingPrice
    {
        get => _startingPrice;
        set
        {
            if (value <= 0)
                throw new DomainException("Starting price must be greater than 0");
            _startingPrice = value;
        }
    }

    public decimal? BuyoutPrice
    {
        get => _buyoutPrice;
        set
        {
            if (value.HasValue && value.Value <= StartingPrice)
                throw new DomainException("Buyout price must be greater than starting price");
            _buyoutPrice = value;
        }
    }

    public decimal? CurrentBid { get; set; }
    public Guid? CurrentBidderId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AuctionStatus Status { get; set; }
    public DateTime CreatedAt { get; private set; }

    public Player? Seller { get; set; }
    public Player? CurrentBidder { get; set; }
    public Artefact? Artefact { get; set; }

    private AuctionListing()
    {
    }

    public AuctionListing(Guid sellerId, Guid artefactId, int quantity, decimal startingPrice, decimal? buyoutPrice, DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future");

        ListingId = Guid.NewGuid();
        SellerId = sellerId;
        ArtefactId = artefactId;
        Quantity = quantity;
        StartingPrice = startingPrice;
        BuyoutPrice = buyoutPrice;
        ExpiresAt = expiresAt;
        Status = AuctionStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
}
