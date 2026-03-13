using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auctions.ListAuctions;

public record ListAuctionsQuery(string? Status, int Page, int Size) : IRequest<Result<ListAuctionsResponse>>;
