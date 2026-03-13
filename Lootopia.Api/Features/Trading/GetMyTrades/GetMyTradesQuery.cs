using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Trading.GetMyTrades;

public record GetMyTradesQuery(Guid UserId, string? Status, int Page, int Size)
    : IRequest<Result<GetMyTradesResponse>>;
