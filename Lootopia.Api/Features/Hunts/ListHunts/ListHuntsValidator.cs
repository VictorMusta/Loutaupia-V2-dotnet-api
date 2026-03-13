using FluentValidation;

namespace Lootopia.Api.Features.Hunts.ListHunts;

public sealed class ListHuntsValidator : AbstractValidator<ListHuntsQuery>
{
    public ListHuntsValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        RuleFor(x => x.RadiusKm)
            .InclusiveBetween(0.1, 500).WithMessage("Radius must be between 0.1 and 500 km.");
    }
}
