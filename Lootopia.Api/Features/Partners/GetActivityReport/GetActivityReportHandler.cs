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

        var campaigns = await db.Campaigns
            .Where(c => c.PartnerId == request.PartnerId)
            .ToListAsync(cancellationToken);

        var campaignIds = campaigns.Select(c => c.Id).ToList();

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

        var couponsDistributed = campaigns.Sum(c => c.CouponsDistributed);
        var activeCampaigns = campaigns.Count(c => c.Status == Domain.Enums.CampaignStatus.Active);

        var totalSpent = campaigns.Sum(c => c.MaxCoupons > 0
            ? (c.CouponsDistributed * c.TokenBudget / c.MaxCoupons)
            : 0);

        var budgetRemaining = partner.TokenBudget - totalSpent;
        if (budgetRemaining < 0) budgetRemaining = 0;

        var campaignStats = campaigns
            .Select(c => new CampaignStatDto(
                c.Id,
                c.Title,
                c.Status.ToString(),
                c.CouponsDistributed,
                c.MaxCoupons,
                c.TokenBudget))
            .ToList();

        var period = new ReportPeriodDto(
            request.From.ToString("yyyy-MM-dd"),
            request.To.ToString("yyyy-MM-dd"));

        return Result.Success(new GetActivityReportResponse(
            partner.Id,
            partner.BusinessName,
            partner.TokenBudget,
            totalSpent,
            activeCampaigns,
            couponsDistributed,
            period,
            totalPlayers,
            budgetRemaining,
            campaignStats));
    }
}
