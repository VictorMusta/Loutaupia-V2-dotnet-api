using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Campaigns.CreateCampaign;

public sealed class CreateCampaignHandler(LootopiaDbContext db) : IRequestHandler<CreateCampaignCommand, Result<CreateCampaignResponse>>
{
    public async Task<Result<CreateCampaignResponse>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var partnerExists = await db.Partners.AnyAsync(p => p.Id == request.PartnerId, cancellationToken);
        if (!partnerExists)
            return Result.Failure<CreateCampaignResponse>(Error.Custom("Partner.NotFound", "Partner not found."));

        if (request.HuntId.HasValue)
        {
            var huntExists = await db.Hunts.AnyAsync(h => h.Id == request.HuntId.Value, cancellationToken);
            if (!huntExists)
                return Result.Failure<CreateCampaignResponse>(Error.Custom("Hunt.NotFound", "Hunt not found."));
        }

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            PartnerId = request.PartnerId,
            Title = request.Title,
            HuntId = request.HuntId,
            TokenBudget = request.TokenBudget,
            MaxCoupons = request.MaxCoupons,
            Status = CampaignStatus.Draft,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCampaignResponse(campaign.Id));
    }
}
