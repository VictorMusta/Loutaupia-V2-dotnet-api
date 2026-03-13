using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auctions.CloseAuction;

public record CloseAuctionCommand(Guid AuctionId, Guid UserId) : IRequest<Result<CloseAuctionResponse>>;
