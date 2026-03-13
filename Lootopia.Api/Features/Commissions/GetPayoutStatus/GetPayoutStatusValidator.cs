using FluentValidation;

namespace Lootopia.Api.Features.Commissions.GetPayoutStatus;

public sealed class GetPayoutStatusValidator : AbstractValidator<GetPayoutStatusQuery>
{
    public GetPayoutStatusValidator()
    {
        RuleFor(x => x.OrganiserId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be at least 1.");
        RuleFor(x => x.Size).InclusiveBetween(1, 100).WithMessage("Size must be between 1 and 100.");
    }
}
