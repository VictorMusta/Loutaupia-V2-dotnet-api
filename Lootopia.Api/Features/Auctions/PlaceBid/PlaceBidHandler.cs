using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auctions.PlaceBid;

public sealed class PlaceBidHandler(
    LootopiaDbContext db,
    INotificationService notificationService) : IRequestHandler<PlaceBidCommand, Result<PlaceBidResponse>>
{
    private const int AntiSnipingMinutes = 2;
    private const int ExtensionMinutes = 5;

    public async Task<Result<PlaceBidResponse>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await db.Auctions
            .Include(a => a.HighestBid)
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction is null)
            return Result.Failure<PlaceBidResponse>(Error.NotFound);

        if (auction.Status != "Active")
            return Result.Failure<PlaceBidResponse>(Error.Custom("Auction.NotActive", "Auction is not active."));

        var now = DateTime.UtcNow;
        if (now >= auction.EndTime)
            return Result.Failure<PlaceBidResponse>(Error.Custom("Auction.Expired", "Auction has ended."));

        if (auction.SellerId == request.BidderId)
            return Result.Failure<PlaceBidResponse>(Error.Custom("Auction.SellerCannotBid", "Seller cannot bid on their own auction."));

        var minBid = auction.ReservePrice;
        if (auction.HighestBid is not null)
            minBid = auction.HighestBid.Amount + auction.MinIncrement;

        if (request.Amount < minBid)
            return Result.Failure<PlaceBidResponse>(Error.Custom("Auction.BidTooLow",
                $"Bid must be at least {minBid} (reserve or current highest + min increment)."));

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == request.BidderId, cancellationToken);
        if (wallet is null || wallet.Balance < request.Amount)
            return Result.Failure<PlaceBidResponse>(Error.Custom("Wallet.InsufficientBalance", "Insufficient wallet balance."));

        DateTime? newEndTime = null;
        if (auction.EndTime.Subtract(now).TotalMinutes <= AntiSnipingMinutes)
        {
            auction.EndTime = now.AddMinutes(ExtensionMinutes);
            newEndTime = auction.EndTime;
        }

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionId = request.AuctionId,
            BidderId = request.BidderId,
            Amount = request.Amount,
            CreatedAt = now
        };

        var previousBidderId = auction.HighestBid?.BidderId;

        db.Bids.Add(bid);
        auction.HighestBidId = bid.Id;
        await db.SaveChangesAsync(cancellationToken);

        if (previousBidderId.HasValue && previousBidderId.Value != request.BidderId)
        {
            var itemName = auction.Item?.Name ?? "un objet";
            await notificationService.SendAsync(
                previousBidderId.Value,
                "Auction",
                "Vous avez été surenchéri",
                $"Quelqu'un a enchéri {request.Amount} LTK sur {itemName}.",
                cancellationToken);
        }

        return Result.Success(new PlaceBidResponse(bid.Id, bid.Amount, newEndTime));
    }
}
