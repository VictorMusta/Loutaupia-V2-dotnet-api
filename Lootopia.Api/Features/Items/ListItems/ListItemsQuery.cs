using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Items.ListItems;

public record ListItemsQuery() : IRequest<Result<ListItemsResponse>>;

public record ItemDto(
    Guid Id,
    string Name,
    string Description,
    string Rarity,
    string Type,
    string? ImageUrl,
    bool IsTradeable);

public record ListItemsResponse(IReadOnlyList<ItemDto> Items);
