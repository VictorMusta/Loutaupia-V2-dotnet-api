using FluentValidation;

namespace Lootopia.Api.Features.Marketplace.CancelListing;

public class CancelListingValidator : AbstractValidator<CancelListingCommand>
{
    public CancelListingValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
