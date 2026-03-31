using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Marketplace.ListListings;

public sealed class ListListingsHandler(LootopiaDbContext db) : IRequestHandler<ListListingsQuery, Result<ListListingsResponse>>
{
    public async Task<Result<ListListingsResponse>> Handle(
        ListListingsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Listings
            .Include(l => l.Item)
            .AsQueryable();

        if (request.SellerId.HasValue)
            query = query.Where(l => l.SellerId == request.SellerId.Value);
        else
            query = query.Where(l => l.Status == "Active");
        if (request.Type.HasValue)
            query = query.Where(l => l.Item.Type == request.Type.Value);
        if (request.Rarity.HasValue)
            query = query.Where(l => l.Item.Rarity == request.Rarity.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Sort?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(l => l.Price),
            "price_desc" => query.OrderByDescending(l => l.Price),
            "created_desc" => query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderBy(l => l.CreatedAt)
        };

        var listings = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(l => new ListingDto(
                l.Id,
                l.SellerId,
                l.ItemId,
                l.Item.Name,
                l.Item.Rarity.ToString(),
                l.Item.Type.ToString(),
                l.Price,
                l.Stock,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new ListListingsResponse(listings, totalCount, request.Page, request.Size));
    }
}
