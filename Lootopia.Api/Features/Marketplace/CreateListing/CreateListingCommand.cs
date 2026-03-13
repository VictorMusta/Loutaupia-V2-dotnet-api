using MediatR;
using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Features.Marketplace.CreateListing;

public record CreateListingCommand(Guid SellerId, Guid ItemId, decimal Price, int Stock)
    : IRequest<Result<CreateListingResponse>>;
