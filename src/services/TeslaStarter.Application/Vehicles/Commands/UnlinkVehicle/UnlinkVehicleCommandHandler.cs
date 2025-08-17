using TeslaStarter.Application.Common.Exceptions;

namespace TeslaStarter.Application.Vehicles.Commands.UnlinkVehicle;

public sealed class UnlinkVehicleCommandHandler(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    ILogger<UnlinkVehicleCommandHandler> logger) : IRequestHandler<UnlinkVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UnlinkVehicleCommandHandler> _logger = logger;
    public async Task Handle(UnlinkVehicleCommand request, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await _vehicleRepository.GetByIdAsync(
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

        _vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Unlinked vehicle {VehicleId}", vehicle.Id.Value);
    }
}
