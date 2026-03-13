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

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(c => new CampaignDto(
                c.Id,
                c.PartnerId,
                c.Title,
                c.HuntId,
                c.TokenBudget,
                c.CouponsDistributed,
                c.MaxCoupons,
                c.Status.ToString(),
                c.ActivatedAt,
                c.ExpiresAt,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetCampaignsResponse(campaigns, totalCount, request.Page, request.Size));
    }
}
