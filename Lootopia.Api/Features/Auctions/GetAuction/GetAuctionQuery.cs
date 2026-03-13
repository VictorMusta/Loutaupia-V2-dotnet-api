using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auctions.GetAuction;

public record GetAuctionQuery(Guid AuctionId) : IRequest<Result<GetAuctionResponse>>;
