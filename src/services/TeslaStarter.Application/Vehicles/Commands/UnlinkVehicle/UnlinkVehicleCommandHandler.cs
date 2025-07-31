using TeslaStarter.Application.Common.Exceptions;

namespace TeslaStarter.Application.Vehicles.Commands.UnlinkVehicle;

public sealed class UnlinkVehicleCommandHandler(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    ILogger<UnlinkVehicleCommandHandler> logger) : IRequestHandler<UnlinkVehicleCommand>
{
    public async Task Handle(UnlinkVehicleCommand request, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await vehicleRepository.GetByIdAsync(
            new VehicleId(request.VehicleId),
            cancellationToken) ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        try
        {
            vehicle.Deactivate();
        }
        catch (InvalidOperationException)
        {
            // Vehicle is already inactive, which is fine - operation is idempotent
        }

        vehicleRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Unlinked vehicle {VehicleId}", vehicle.Id.Value);
    }
}
