using FluentValidation;

namespace Lootopia.Api.Features.Wallet.CreditWallet;

public class CreditWalletValidator : AbstractValidator<CreditWalletCommand>
{
    public CreditWalletValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.IdempotencyKey).MaximumLength(128).When(x => !string.IsNullOrEmpty(x.IdempotencyKey));
    }
}
