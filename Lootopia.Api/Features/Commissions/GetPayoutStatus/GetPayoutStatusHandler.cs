using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Commissions.GetPayoutStatus;

public sealed class GetPayoutStatusHandler(LootopiaDbContext db)
    : IRequestHandler<GetPayoutStatusQuery, Result<GetPayoutStatusResponse>>
{
    public async Task<Result<GetPayoutStatusResponse>> Handle(GetPayoutStatusQuery request, CancellationToken cancellationToken)
    {
        var query = db.Payouts.Where(p => p.OrganiserId == request.OrganiserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var payouts = await query
            .OrderByDescending(p => p.RequestedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(p => new PayoutDto(
                p.Id,
                p.Amount,
                p.Status,
                p.RequestedAt,
                p.SettledAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetPayoutStatusResponse(payouts, totalCount));
    }
}
