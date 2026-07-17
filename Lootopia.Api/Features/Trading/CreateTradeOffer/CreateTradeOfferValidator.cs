using FluentValidation;

namespace Lootopia.Api.Features.Trading.CreateTradeOffer;

public sealed class CreateTradeOfferValidator : AbstractValidator<CreateTradeOfferCommand>
{
    public CreateTradeOfferValidator()
    {
        RuleFor(x => x.InitiatorId).NotEmpty();
        RuleFor(x => x.ReceiverId).NotEmpty();
        RuleFor(x => x.ExpiresAt).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x)
            .Must(x => x.OfferedItems.Count > 0 || x.RequestedItems.Count > 0)
            .WithMessage("At least one offered or requested item is required.");
    }
}
