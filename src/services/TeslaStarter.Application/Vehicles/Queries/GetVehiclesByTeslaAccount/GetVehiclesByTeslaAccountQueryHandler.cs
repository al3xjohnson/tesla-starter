using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehiclesByTeslaAccount;

public sealed class GetVehiclesByTeslaAccountQueryHandler(
    IVehicleRepository vehicleRepository,
    IMapper mapper) : IRequestHandler<GetVehiclesByTeslaAccountQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<IReadOnlyList<VehicleDto>> Handle(GetVehiclesByTeslaAccountQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Vehicle> vehicles = await _vehicleRepository.GetByTeslaAccountIdAsync(
            TeslaAccountId.Create(request.TeslaAccountId),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);
    }
}
