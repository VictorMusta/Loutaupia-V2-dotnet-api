using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Items.ListItems;

public sealed class ListItemsHandler(LootopiaDbContext db) : IRequestHandler<ListItemsQuery, Result<ListItemsResponse>>
{
    public async Task<Result<ListItemsResponse>> Handle(ListItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Items
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => new ItemDto(
            i.Id,
            i.Name,
            i.Description,
            i.Rarity.ToString(),
            i.Type.ToString(),
            i.ImageUrl,
            i.IsTradeable)).ToList();

        return Result.Success(new ListItemsResponse(dtos));
    }
}
