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
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction is null)
            return Result.Failure<GetAuctionResponse>(Error.NotFound);

        var bidCount = await db.Bids.CountAsync(b => b.AuctionId == request.AuctionId, cancellationToken);

        return Result.Success(new GetAuctionResponse(
            auction.Id,
            auction.SellerId,
            auction.ItemId,
            auction.ReservePrice,
            auction.MinIncrement,
            auction.StartTime,
            auction.EndTime,
            auction.Status,
            auction.HighestBid?.Amount,
            bidCount,
            auction.Item?.Name,
            auction.Item?.ImageUrl));
    }
}
