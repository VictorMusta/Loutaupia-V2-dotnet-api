using FluentValidation;

namespace Lootopia.Api.Features.Admin.UnfreezeUser;

public sealed class UnfreezeUserValidator : AbstractValidator<UnfreezeUserCommand>
{
    public UnfreezeUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
