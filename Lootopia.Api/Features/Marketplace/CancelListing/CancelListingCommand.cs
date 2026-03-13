using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Marketplace.CancelListing;

public record CancelListingCommand(Guid ListingId, Guid UserId) : IRequest<Result>;
