using FluentValidation;

namespace Lootopia.Api.Features.Items.CreateItem;

public sealed class CreateItemValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Rarity).IsInEnum();
        RuleFor(x => x.Type).IsInEnum();
    }
}
