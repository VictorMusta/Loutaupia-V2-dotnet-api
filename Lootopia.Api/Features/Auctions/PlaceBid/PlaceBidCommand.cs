using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auctions.PlaceBid;

public record PlaceBidCommand(Guid AuctionId, Guid BidderId, decimal Amount) : IRequest<Result<PlaceBidResponse>>;
