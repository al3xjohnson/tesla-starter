namespace TeslaStarter.Application.Vehicles.Commands.UnlinkVehicle;

public record UnlinkVehicleCommand : IRequest
{
    public Guid VehicleId { get; init; }
}
