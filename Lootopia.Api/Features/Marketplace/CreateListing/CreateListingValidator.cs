using FluentValidation;

namespace Lootopia.Api.Features.Marketplace.CreateListing;

public class CreateListingValidator : AbstractValidator<CreateListingCommand>
{
    public CreateListingValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be positive.");
        RuleFor(x => x.Stock).GreaterThan(0).WithMessage("Stock must be positive.");
    }
}
