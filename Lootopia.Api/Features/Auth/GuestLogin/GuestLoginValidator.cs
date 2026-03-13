using FluentValidation;

namespace Lootopia.Api.Features.Auth.GuestLogin;

public sealed class GuestLoginValidator : AbstractValidator<GuestLoginCommand>
{
    public GuestLoginValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.")
            .MinimumLength(8).WithMessage("Device ID must be at least 8 characters.");
    }
}
