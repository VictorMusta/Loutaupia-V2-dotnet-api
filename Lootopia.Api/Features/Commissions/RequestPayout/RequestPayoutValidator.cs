using FluentValidation;

namespace Lootopia.Api.Features.Commissions.RequestPayout;

public sealed class RequestPayoutValidator : AbstractValidator<RequestPayoutCommand>
{
    public RequestPayoutValidator()
    {
        RuleFor(x => x.OrganiserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
    }
}
