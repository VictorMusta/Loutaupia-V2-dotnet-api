using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Marketplace.PurchaseItem;

public record PurchaseItemCommand(Guid ListingId, Guid BuyerId, int Quantity, string? IdempotencyKey)
    : IRequest<Result<PurchaseItemResponse>>;
