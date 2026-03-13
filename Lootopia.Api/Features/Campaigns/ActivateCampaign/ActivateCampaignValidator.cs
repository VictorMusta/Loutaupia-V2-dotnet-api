using FluentValidation;

namespace Lootopia.Api.Features.Campaigns.ActivateCampaign;

public sealed class ActivateCampaignValidator : AbstractValidator<ActivateCampaignCommand>
{
    public ActivateCampaignValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.PartnerId).NotEmpty();
    }
}
