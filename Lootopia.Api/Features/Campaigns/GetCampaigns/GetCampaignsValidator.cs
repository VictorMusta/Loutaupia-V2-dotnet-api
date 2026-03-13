using FluentValidation;

namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public sealed class GetCampaignsValidator : AbstractValidator<GetCampaignsQuery>
{
    public GetCampaignsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
    }
}
