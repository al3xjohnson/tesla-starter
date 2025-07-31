using Common.Domain.Persistence;

namespace TeslaStarter.Domain.Vehicles;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByTeslaAccountIdAsync(TeslaAccountId teslaAccountId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByVehicleIdentifierAsync(string vehicleIdentifier, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(VehicleId id, CancellationToken cancellationToken = default);
}
