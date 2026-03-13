using FluentValidation;

namespace Lootopia.Api.Features.Admin.FreezeCampaign;

public sealed class FreezeCampaignValidator : AbstractValidator<FreezeCampaignCommand>
{
    public FreezeCampaignValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
    }
}
