using Lootopia.Api.Domain.Enums;
using MediatR;
using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Features.Inventory.GetInventory;

public record GetInventoryQuery(
    Guid PlayerId,
    ItemType? Type,
    ItemRarity? Rarity,
    int Page,
    int Size) : IRequest<Result<GetInventoryResponse>>;
