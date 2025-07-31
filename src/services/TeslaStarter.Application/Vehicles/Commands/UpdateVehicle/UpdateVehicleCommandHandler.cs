using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandHandler(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateVehicleCommandHandler> logger) : IRequestHandler<UpdateVehicleCommand, VehicleDto>
{
    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        Vehicle vehicle = await vehicleRepository.GetByIdAsync(
            new VehicleId(request.VehicleId),
            cancellationToken) ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        vehicle.UpdateDisplayName(request.DisplayName);

        vehicleRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated vehicle {VehicleId} display name", vehicle.Id.Value);

        return mapper.Map<VehicleDto>(vehicle);
    }
}
