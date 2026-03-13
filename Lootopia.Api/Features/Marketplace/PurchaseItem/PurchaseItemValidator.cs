using FluentValidation;

namespace Lootopia.Api.Features.Marketplace.PurchaseItem;

public class PurchaseItemValidator : AbstractValidator<PurchaseItemCommand>
{
    public PurchaseItemValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive.");
    }
}
