using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Campaigns.ActivateCampaign;

public sealed class ActivateCampaignHandler(LootopiaDbContext db) : IRequestHandler<ActivateCampaignCommand, Result>
{
    public async Task<Result> Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Partner)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId && c.PartnerId == request.PartnerId, cancellationToken);

        if (campaign is null)
            return Result.Failure(Error.Custom("Campaign.NotFound", "Campaign not found."));

        if (campaign.Status != CampaignStatus.Draft)
            return Result.Failure(Error.Custom("Campaign.InvalidStatus", "Only draft campaigns can be activated."));

        if (campaign.Partner.TokenBudget < campaign.TokenBudget)
            return Result.Failure(Error.Custom("Campaign.InsufficientBudget", "Partner token budget is insufficient."));

        campaign.Status = CampaignStatus.Active;
        campaign.ActivatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
