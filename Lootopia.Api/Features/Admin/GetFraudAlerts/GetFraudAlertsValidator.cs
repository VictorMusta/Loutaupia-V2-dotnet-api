using FluentValidation;

namespace Lootopia.Api.Features.Admin.GetFraudAlerts;

public sealed class GetFraudAlertsValidator : AbstractValidator<GetFraudAlertsQuery>
{
    public GetFraudAlertsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
    }
}
