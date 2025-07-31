using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Commands.LinkVehicle;

public sealed class LinkVehicleCommandHandler(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<LinkVehicleCommandHandler> logger) : IRequestHandler<LinkVehicleCommand, VehicleDto>
{
    public async Task<VehicleDto> Handle(LinkVehicleCommand request, CancellationToken cancellationToken)
    {

        // Check if vehicle already exists with this identifier
        Vehicle? existingVehicle = await vehicleRepository.GetByVehicleIdentifierAsync(
            request.VehicleIdentifier,
            cancellationToken);

        if (existingVehicle != null && existingVehicle.TeslaAccountId.Value == request.TeslaAccountId)
        {
            throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.VehicleIdentifier),
                    "This vehicle is already linked to this Tesla account.")
            ]);
        }

        // Create the vehicle
        Vehicle vehicle = Vehicle.Link(
            TeslaAccountId.Create(request.TeslaAccountId),
            request.VehicleIdentifier,
            request.DisplayName);

        vehicleRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Linked vehicle {VehicleIdentifier} to Tesla account {TeslaAccountId}",
            vehicle.VehicleIdentifier, vehicle.TeslaAccountId.Value);

        return mapper.Map<VehicleDto>(vehicle);
    }
}
