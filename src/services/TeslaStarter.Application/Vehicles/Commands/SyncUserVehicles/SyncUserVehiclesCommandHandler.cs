using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Common.Interfaces;

namespace TeslaStarter.Application.Vehicles.Commands.SyncUserVehicles;

public sealed class SyncUserVehiclesCommandHandler(
    IUserRepository userRepository,
    IVehicleRepository vehicleRepository,
    ITeslaApiService teslaApiService,
    IUnitOfWork unitOfWork,
    ILogger<SyncUserVehiclesCommandHandler> logger) : IRequestHandler<SyncUserVehiclesCommand, int>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    private readonly ITeslaApiService _teslaApiService = teslaApiService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<SyncUserVehiclesCommandHandler> _logger = logger;

    public async Task<int> Handle(SyncUserVehiclesCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByExternalIdAsync(
            ExternalId.Create(request.ExternalId),
            cancellationToken) ?? throw new NotFoundException(nameof(User), request.ExternalId);

        if (user.TeslaAccount?.AccessToken == null)
        {
            _logger.LogWarning("User {UserId} has no Tesla account linked", user.Id.Value);
            return 0;
        }

        // Fetch vehicles from Tesla API
        IReadOnlyList<TeslaVehicleDto> teslaVehicles = await _teslaApiService.GetVehiclesAsync(user.TeslaAccount.AccessToken!);

        _logger.LogInformation("Found {Count} vehicles for user {UserId}", teslaVehicles.Count, user.Id.Value);

        int syncedCount = 0;

        foreach (TeslaVehicleDto teslaVehicle in teslaVehicles)
        {
            // Check if vehicle already exists
            Vehicle? existingVehicle = await _vehicleRepository.GetByVehicleIdentifierAsync(
                teslaVehicle.Vin,
                cancellationToken);

            if (existingVehicle == null)
            {
                // Create new vehicle
                Vehicle vehicle = Vehicle.Link(
                    user.TeslaAccount.TeslaAccountId,
                    teslaVehicle.Vin,
                    string.IsNullOrEmpty(teslaVehicle.DisplayName) ? null : teslaVehicle.DisplayName);

                _vehicleRepository.Add(vehicle);
                syncedCount++;

                _logger.LogInformation("Added new vehicle {VIN} for user {UserId}",
                    teslaVehicle.Vin, user.Id.Value);
            }
            else if (existingVehicle.TeslaAccountId == user.TeslaAccount.TeslaAccountId)
            {
                // Update existing vehicle
                existingVehicle.UpdateDisplayName(teslaVehicle.DisplayName);
                existingVehicle.RecordSync();

                _vehicleRepository.Update(existingVehicle);
                syncedCount++;

                _logger.LogInformation("Updated vehicle {VIN} for user {UserId}",
                    teslaVehicle.Vin, user.Id.Value);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return syncedCount;
    }
}
