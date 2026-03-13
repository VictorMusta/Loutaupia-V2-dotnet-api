using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.FreezeCampaign;

public sealed class FreezeCampaignHandler(LootopiaDbContext db) : IRequestHandler<FreezeCampaignCommand, Result>
{
    public async Task<Result> Handle(FreezeCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns.FindAsync([request.CampaignId], cancellationToken);
        if (campaign is null)
            return Result.Failure(Error.Custom("Campaign.NotFound", "Campaign not found."));

        campaign.Status = CampaignStatus.Frozen;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
