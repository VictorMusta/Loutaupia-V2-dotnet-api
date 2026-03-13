using FluentValidation;

namespace Lootopia.Api.Features.Auctions.ListAuctions;

public sealed class ListAuctionsValidator : AbstractValidator<ListAuctionsQuery>
{
    public ListAuctionsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be at least 1.");
        RuleFor(x => x.Size).InclusiveBetween(1, 100).WithMessage("Size must be between 1 and 100.");
    }
}
