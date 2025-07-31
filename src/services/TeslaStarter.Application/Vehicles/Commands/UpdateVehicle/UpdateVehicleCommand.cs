using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Commands.UpdateVehicle;

public record UpdateVehicleCommand : IRequest<VehicleDto>
{
    public Guid VehicleId { get; init; }
    public string? DisplayName { get; init; }
}
