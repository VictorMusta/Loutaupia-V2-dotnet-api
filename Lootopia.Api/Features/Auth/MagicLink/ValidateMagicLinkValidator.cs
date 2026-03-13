using FluentValidation;

namespace Lootopia.Api.Features.Auth.MagicLink;

public sealed class ValidateMagicLinkValidator : AbstractValidator<ValidateMagicLinkCommand>
{
    public ValidateMagicLinkValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Magic link token is required.");
    }
}
