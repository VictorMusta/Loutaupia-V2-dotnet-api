using FluentValidation;

namespace Lootopia.Api.Features.Campaigns.CreateCampaign;

public sealed class CreateCampaignValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TokenBudget).GreaterThan(0);
        RuleFor(x => x.MaxCoupons).GreaterThan(0);
    }
}
