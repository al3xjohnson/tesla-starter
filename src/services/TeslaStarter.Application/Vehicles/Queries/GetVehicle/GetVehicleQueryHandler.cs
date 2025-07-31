using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Queries.GetVehicle;

public sealed class GetVehicleQueryHandler(
    IVehicleRepository vehicleRepository,
    IMapper mapper) : IRequestHandler<GetVehicleQuery, VehicleDto>
{
    public async Task<VehicleDto> Handle(GetVehicleQuery request, CancellationToken cancellationToken)
    {
        Vehicle vehicle = await vehicleRepository.GetByIdAsync(
            new VehicleId(request.VehicleId),
            cancellationToken) ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        return mapper.Map<VehicleDto>(vehicle);
    }
}
