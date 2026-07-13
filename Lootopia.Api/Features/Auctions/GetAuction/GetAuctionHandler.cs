using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auctions.GetAuction;

public sealed class GetAuctionHandler(LootopiaDbContext db) : IRequestHandler<GetAuctionQuery, Result<GetAuctionResponse>>
{
    public async Task<Result<GetAuctionResponse>> Handle(GetAuctionQuery request, CancellationToken cancellationToken)
    {
        var auction = await db.Auctions
            .Include(a => a.Item)
            .Include(a => a.HighestBid)
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction is null)
            return Result.Failure<GetAuctionResponse>(Error.NotFound);

        var bids = await db.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == request.AuctionId)
            .OrderByDescending(b => b.Amount)
            .ThenByDescending(b => b.CreatedAt)
            .Select(b => new BidDto(
                b.Id,
                b.BidderId,
                b.Bidder.DisplayName,
                b.Amount,
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        var currentPrice = auction.HighestBid?.Amount ?? auction.ReservePrice;
        var minBid = auction.HighestBid is not null
            ? auction.HighestBid.Amount + auction.MinIncrement
            : auction.ReservePrice;

        return Result.Success(new GetAuctionResponse(
            auction.Id,
            auction.SellerId,
            auction.Seller?.DisplayName,
            auction.ItemId,
            auction.ReservePrice,
            auction.MinIncrement,
            auction.StartTime,
            auction.EndTime,
            auction.Status,
            auction.HighestBid?.Amount,
            currentPrice,
            minBid,
            bids.Count,
            auction.Item?.Name,
            auction.Item?.ImageUrl,
            bids));
    }
}
