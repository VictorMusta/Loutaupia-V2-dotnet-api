using FluentValidation;

namespace Lootopia.Api.Features.Admin.FreezeUser;

public sealed class FreezeUserValidator : AbstractValidator<FreezeUserCommand>
{
    public FreezeUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
