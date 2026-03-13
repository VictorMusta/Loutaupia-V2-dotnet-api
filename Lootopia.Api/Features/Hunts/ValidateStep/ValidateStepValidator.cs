using FluentValidation;

namespace Lootopia.Api.Features.Hunts.ValidateStep;

public sealed class ValidateStepValidator : AbstractValidator<ValidateStepCommand>
{
    public ValidateStepValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.HuntId).NotEmpty();
        RuleFor(x => x.StepOrder).GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
