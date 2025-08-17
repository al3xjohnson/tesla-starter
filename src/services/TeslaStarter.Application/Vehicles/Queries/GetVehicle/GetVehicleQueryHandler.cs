using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehicle;

public sealed class GetVehicleQueryHandler(
    IVehicleRepository vehicleRepository,
    IMapper mapper) : IRequestHandler<GetVehicleQuery, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<VehicleDto> Handle(GetVehicleQuery request, CancellationToken cancellationToken)
    {
        Vehicle vehicle = await _vehicleRepository.GetByIdAsync(
            new VehicleId(request.VehicleId),
            cancellationToken) ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        return _mapper.Map<VehicleDto>(vehicle);
    }
}
