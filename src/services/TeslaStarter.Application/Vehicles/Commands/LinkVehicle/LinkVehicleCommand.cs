using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Commands.LinkVehicle;

public record LinkVehicleCommand : IRequest<VehicleDto>
{
    public string TeslaAccountId { get; init; } = string.Empty;
    public string VehicleIdentifier { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
}
