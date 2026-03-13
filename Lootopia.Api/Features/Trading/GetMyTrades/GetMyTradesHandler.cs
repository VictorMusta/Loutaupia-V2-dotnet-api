using Lootopia.Api.Features.Trading.GetMyTrades;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Trading.GetMyTrades;

public sealed class GetMyTradesHandler(LootopiaDbContext db) : IRequestHandler<GetMyTradesQuery, Result<GetMyTradesResponse>>
{
    public async Task<Result<GetMyTradesResponse>> Handle(
        GetMyTradesQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.TradeOffers
            .Where(t => t.InitiatorId == request.UserId || t.ReceiverId == request.UserId)
            .Include(t => t.Items)
            .ThenInclude(i => i.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(t => t.Status == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var offers = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var dtos = offers.Select(t => new TradeOfferDto(
            t.Id,
            t.InitiatorId,
            t.ReceiverId,
            t.Status,
            t.ExpiresAt,
            t.CreatedAt,
            t.Items.Where(i => i.Side == "Offered")
                .Select(i => new TradeOfferItemDto(i.ItemId, i.Item?.Name, i.Quantity, i.TokenAmount))
                .ToList(),
            t.Items.Where(i => i.Side == "Requested")
                .Select(i => new TradeOfferItemDto(i.ItemId, i.Item?.Name, i.Quantity, i.TokenAmount))
                .ToList()
        )).ToList();

        return Result.Success(new GetMyTradesResponse(dtos, totalCount, request.Page, request.Size));
    }
}
