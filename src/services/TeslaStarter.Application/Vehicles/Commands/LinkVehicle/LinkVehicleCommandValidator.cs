namespace TeslaStarter.Application.Vehicles.Commands.LinkVehicle;

public sealed class LinkVehicleCommandValidator : AbstractValidator<LinkVehicleCommand>
{
    public LinkVehicleCommandValidator()
    {
        RuleFor(x => x.TeslaAccountId)
            .NotEmpty().WithMessage("Tesla account ID is required.")
            .MaximumLength(255).WithMessage("Tesla account ID must not exceed 255 characters.");

        RuleFor(x => x.VehicleIdentifier)
            .NotEmpty().WithMessage("Vehicle identifier is required.")
            .MaximumLength(50).WithMessage("Vehicle identifier must not exceed 50 characters.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}
