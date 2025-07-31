using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehicle;

public record GetVehicleQuery : IRequest<VehicleDto>
{
    public Guid VehicleId { get; init; }
}
