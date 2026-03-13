using FluentValidation;

namespace Lootopia.Api.Features.Partners.GetActivityReport;

public sealed class GetActivityReportValidator : AbstractValidator<GetActivityReportQuery>
{
    public GetActivityReportValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.From).LessThanOrEqualTo(x => x.To).WithMessage("From date must be before or equal to To date.");
    }
}
