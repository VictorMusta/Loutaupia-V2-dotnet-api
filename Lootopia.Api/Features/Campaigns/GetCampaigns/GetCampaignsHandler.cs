using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public sealed class GetCampaignsHandler(LootopiaDbContext db) : IRequestHandler<GetCampaignsQuery, Result<GetCampaignsResponse>>
{
    public async Task<Result<GetCampaignsResponse>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Campaigns.AsQueryable();

        if (!request.AdminView && request.PartnerId.HasValue)
            query = query.Where(c => c.PartnerId == request.PartnerId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var campaignsData = await query
            .Include(c => c.Partner)
            .Include(c => c.Hunt)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var campaigns = campaignsData
            .Select(c => new CampaignDto(
                c.Id,
                c.PartnerId,
                c.Partner?.BusinessName ?? "Unknown Partner",
                c.Title,
                c.Hunt?.Description ?? "Campagne de récompenses partagées",
                c.TokenBudget,
                c.MaxCoupons > 0 ? (c.CouponsDistributed * c.TokenBudget / c.MaxCoupons) : 0,
                c.Status.ToString(),
                c.ActivatedAt,
                c.ExpiresAt,
                c.CreatedAt))
            .ToList();

        return Result.Success(new GetCampaignsResponse(campaigns, totalCount, request.Page, request.Size));
    }
}
