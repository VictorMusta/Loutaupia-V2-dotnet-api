using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Inventory.GetInventory;

public sealed class GetInventoryHandler(LootopiaDbContext db) : IRequestHandler<GetInventoryQuery, Result<GetInventoryResponse>>
{
    public async Task<Result<GetInventoryResponse>> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        var query = db.PlayerInventories
            .Where(pi => pi.PlayerId == request.PlayerId)
            .Include(pi => pi.Item)
            .AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(pi => pi.Item.Type == request.Type.Value);
        if (request.Rarity.HasValue)
            query = query.Where(pi => pi.Item.Rarity == request.Rarity.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(pi => pi.AcquiredAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(pi => new InventoryItemDto(
                pi.ItemId,
                pi.Item.Name,
                pi.Item.Description,
                pi.Item.Rarity.ToString(),
                pi.Item.Type.ToString(),
                pi.Item.ImageUrl,
                pi.Quantity,
                pi.Item.IsTradeable,
                pi.AcquiredAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetInventoryResponse(items, totalCount, request.Page, request.Size));
    }
}
