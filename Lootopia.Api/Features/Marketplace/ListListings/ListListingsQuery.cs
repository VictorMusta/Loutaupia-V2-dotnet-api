using Lootopia.Api.Domain.Enums;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Marketplace.ListListings;

public record ListListingsQuery(
    ItemType? Type,
    ItemRarity? Rarity,
    string? Sort,
    int Page,
    int Size,
    Guid? SellerId = null) : IRequest<Result<ListListingsResponse>>;
