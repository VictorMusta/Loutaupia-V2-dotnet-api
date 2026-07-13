using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auctions.ListAuctions;

public sealed class ListAuctionsHandler(LootopiaDbContext db) : IRequestHandler<ListAuctionsQuery, Result<ListAuctionsResponse>>
{
    public async Task<Result<ListAuctionsResponse>> Handle(ListAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Auctions
            .Include(a => a.Item)
            .Include(a => a.HighestBid)
            .Include(a => a.Seller)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(a => a.Status == request.Status);
        else
            query = query.Where(a => a.Status == "Active" && a.EndTime > DateTime.UtcNow);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(a => new AuctionSummary(
                a.Id,
                a.SellerId,
                a.ItemId,
                a.ReservePrice,
                a.MinIncrement,
                a.EndTime,
                a.Status,
                a.HighestBid != null ? a.HighestBid.Amount : (decimal?)null,
                a.HighestBid != null ? a.HighestBid.Amount : a.ReservePrice,
                a.Bids.Count,
                a.Item != null ? a.Item.Name : null,
                a.Item != null ? a.Item.Rarity.ToString() : null,
                a.Item != null ? a.Item.ImageUrl : null,
                a.Seller != null ? a.Seller.DisplayName : null))
            .ToListAsync(cancellationToken);

        return Result.Success(new ListAuctionsResponse(items, totalCount));
    }
}
