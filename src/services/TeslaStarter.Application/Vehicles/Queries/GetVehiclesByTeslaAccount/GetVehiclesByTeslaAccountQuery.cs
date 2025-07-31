using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehiclesByTeslaAccount;

public record GetVehiclesByTeslaAccountQuery : IRequest<IReadOnlyList<VehicleDto>>
{
    public string TeslaAccountId { get; init; } = string.Empty;
}
