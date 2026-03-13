using FluentValidation;

namespace Lootopia.Api.Features.Hunts.ActivateHunt;

public sealed class ActivateHuntValidator : AbstractValidator<ActivateHuntCommand>
{
    public ActivateHuntValidator()
    {
        RuleFor(x => x.HuntId).NotEmpty();
    }
}
