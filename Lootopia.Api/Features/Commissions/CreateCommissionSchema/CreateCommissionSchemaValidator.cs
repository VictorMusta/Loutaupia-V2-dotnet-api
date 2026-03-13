using FluentValidation;

namespace Lootopia.Api.Features.Commissions.CreateCommissionSchema;

public sealed class CreateCommissionSchemaValidator : AbstractValidator<CreateCommissionSchemaCommand>
{
    public CreateCommissionSchemaValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t is "Fixed" or "Percentage" or "Split")
            .WithMessage("Type must be Fixed, Percentage, or Split.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).WithMessage("Value must be non-negative.");
        RuleFor(x => x.PlatformShare).InclusiveBetween(0, 1).WithMessage("Platform share must be between 0 and 1.");
        RuleFor(x => x.OrganiserShare).InclusiveBetween(0, 1).WithMessage("Organiser share must be between 0 and 1.");
        RuleFor(x => x).Must(x => Math.Abs(x.PlatformShare + x.OrganiserShare - 1) < 0.0001m)
            .WithMessage("Platform share and Organiser share must sum to 1.");
    }
}
