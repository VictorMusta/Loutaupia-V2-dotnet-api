using Lootopia.Api.Domain.Enums;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Items.CreateItem;

public record CreateItemCommand(
    string Name,
    string Description,
    ItemRarity Rarity,
    ItemType Type,
    string? ImageUrl,
    bool IsTradeable) : IRequest<Result<CreateItemResponse>>;

public record CreateItemResponse(Guid ItemId);
