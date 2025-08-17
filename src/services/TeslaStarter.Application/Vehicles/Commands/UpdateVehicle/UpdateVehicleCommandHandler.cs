using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Vehicles.DTOs;

namespace TeslaStarter.Application.Vehicles.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandHandler(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateVehicleCommandHandler> logger) : IRequestHandler<UpdateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UpdateVehicleCommandHandler> _logger = logger;
    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        Vehicle vehicle = await _vehicleRepository.GetByIdAsync(
            new VehicleId(request.VehicleId),
            cancellationToken) ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        vehicle.UpdateDisplayName(request.DisplayName);

        _vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated vehicle {VehicleId} display name", vehicle.Id.Value);

        return _mapper.Map<VehicleDto>(vehicle);
    }
}
