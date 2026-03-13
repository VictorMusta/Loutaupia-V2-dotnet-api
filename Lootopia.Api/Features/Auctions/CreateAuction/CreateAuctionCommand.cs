using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auctions.CreateAuction;

public record CreateAuctionCommand(
    Guid SellerId,
    Guid ItemId,
    decimal ReservePrice,
    decimal MinIncrement,
    int DurationMinutes) : IRequest<Result<CreateAuctionResponse>>;
