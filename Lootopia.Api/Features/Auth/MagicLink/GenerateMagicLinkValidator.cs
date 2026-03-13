using FluentValidation;

namespace Lootopia.Api.Features.Auth.MagicLink;

public sealed class GenerateMagicLinkValidator : AbstractValidator<GenerateMagicLinkCommand>
{
    public GenerateMagicLinkValidator()
    {
        RuleFor(x => x.PartnerUserId)
            .NotEmpty().WithMessage("Partner user ID is required.");

        RuleFor(x => x.RequestingAdminId)
            .NotEmpty().WithMessage("Requesting admin ID is required.");
    }
}
