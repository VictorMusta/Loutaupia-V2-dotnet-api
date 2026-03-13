using FluentValidation;

namespace Lootopia.Api.Features.Auctions.PlaceBid;

public sealed class PlaceBidValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty();
        RuleFor(x => x.BidderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Bid amount must be positive.");
    }
}
