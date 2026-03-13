using FluentValidation;

namespace Lootopia.Api.Features.Auctions.CreateAuction;

public sealed class CreateAuctionValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.ReservePrice).GreaterThanOrEqualTo(0).WithMessage("Reserve price must be non-negative.");
        RuleFor(x => x.MinIncrement).GreaterThan(0).WithMessage("Min increment must be positive.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 10080).WithMessage("Duration must be between 1 and 10080 minutes (7 days).");
    }
}
