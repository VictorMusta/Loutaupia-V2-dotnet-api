using FluentValidation;

namespace Lootopia.Api.Features.Trading.RespondToTrade;

public class RespondToTradeValidator : AbstractValidator<RespondToTradeCommand>
{
    public RespondToTradeValidator()
    {
        RuleFor(x => x.OfferId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Action)
            .Must(a => a is "accept" or "refuse")
            .WithMessage("Action must be 'accept' or 'refuse'.");
    }
}
