using FluentValidation;

namespace Lootopia.Api.Features.Hunts.GetHunt;

public sealed class GetHuntValidator : AbstractValidator<GetHuntQuery>
{
    public GetHuntValidator()
    {
        RuleFor(x => x.HuntId).NotEmpty();
    }
}
