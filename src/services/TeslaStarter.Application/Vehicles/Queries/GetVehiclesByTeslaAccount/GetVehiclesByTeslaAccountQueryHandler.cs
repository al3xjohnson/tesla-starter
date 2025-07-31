using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehiclesByTeslaAccount;

public sealed class GetVehiclesByTeslaAccountQueryHandler(
    IVehicleRepository vehicleRepository,
    IMapper mapper) : IRequestHandler<GetVehiclesByTeslaAccountQuery, IReadOnlyList<VehicleDto>>
{
    public async Task<IReadOnlyList<VehicleDto>> Handle(GetVehiclesByTeslaAccountQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Vehicle> vehicles = await vehicleRepository.GetByTeslaAccountIdAsync(
            TeslaAccountId.Create(request.TeslaAccountId),
            cancellationToken);

        return mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);
    }
}
