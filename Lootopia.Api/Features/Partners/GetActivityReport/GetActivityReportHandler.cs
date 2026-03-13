using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Partners.GetActivityReport;

public sealed class GetActivityReportHandler(LootopiaDbContext db) : IRequestHandler<GetActivityReportQuery, Result<GetActivityReportResponse>>
{
    public async Task<Result<GetActivityReportResponse>> Handle(GetActivityReportQuery request, CancellationToken cancellationToken)
    {
        var partner = await db.Partners.FindAsync([request.PartnerId], cancellationToken);
        if (partner is null)
            return Result.Failure<GetActivityReportResponse>(Error.Custom("Partner.NotFound", "Partner not found."));

        var campaignIds = await db.Campaigns
            .Where(c => c.PartnerId == request.PartnerId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var couponsDistributed = await db.Campaigns
            .Where(c => c.PartnerId == request.PartnerId)
            .SumAsync(c => c.CouponsDistributed, cancellationToken);

        var totalPlayers = 0;
        if (campaignIds.Count > 0)
        {
            totalPlayers = await db.PlayerHunts
                .Where(ph => campaignIds.Contains(ph.HuntId))
                .Where(ph => ph.StartedAt >= request.From && ph.StartedAt <= request.To)
                .Select(ph => ph.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

        var budgetConsumed = await db.Campaigns
            .Where(c => c.PartnerId == request.PartnerId)
            .SumAsync(c => c.CouponsDistributed * c.TokenBudget / Math.Max(c.MaxCoupons, 1), cancellationToken);

        var budgetRemaining = partner.TokenBudget - budgetConsumed;
        if (budgetRemaining < 0) budgetRemaining = 0;

        var campaignStats = await db.Campaigns
            .Where(c => c.PartnerId == request.PartnerId)
            .Select(c => new CampaignStatDto(
                c.Id,
                c.Title,
                c.Status.ToString(),
                c.CouponsDistributed,
                c.MaxCoupons,
                c.TokenBudget))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetActivityReportResponse(
            totalPlayers,
            couponsDistributed,
            budgetRemaining,
            campaignStats));
    }
}
