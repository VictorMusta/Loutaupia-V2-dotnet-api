using FluentValidation;

namespace Lootopia.Api.Features.Admin.CreditPartnerBudget;

public sealed class CreditPartnerBudgetValidator : AbstractValidator<CreditPartnerBudgetCommand>
{
    public CreditPartnerBudgetValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
    }
}
