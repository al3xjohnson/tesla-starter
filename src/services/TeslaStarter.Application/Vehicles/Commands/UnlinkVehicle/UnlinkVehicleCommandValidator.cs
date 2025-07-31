namespace TeslaStarter.Application.Vehicles.Commands.UnlinkVehicle;

public sealed class UnlinkVehicleCommandValidator : AbstractValidator<UnlinkVehicleCommand>
{
    public UnlinkVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle ID is required.");
    }
}
