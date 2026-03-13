using FluentValidation;

namespace Lootopia.Api.Features.Hunts.CreateHunt;

public sealed class CreateHuntValidator : AbstractValidator<CreateHuntCommand>
{
    public CreateHuntValidator()
    {
        RuleFor(x => x.CreatedBy).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Difficulty).InclusiveBetween(1, 5);
        RuleFor(x => x.RewardTokens).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one step is required.");
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Latitude).InclusiveBetween(-90, 90);
            step.RuleFor(s => s.Longitude).InclusiveBetween(-180, 180);
            step.RuleFor(s => s.RadiusMeters).InclusiveBetween(1, 500);
            step.RuleFor(s => s.Clue).NotEmpty().MaximumLength(500);
        });
    }
}
