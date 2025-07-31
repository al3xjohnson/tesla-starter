using Microsoft.EntityFrameworkCore;
using TeslaStarter.Domain.Vehicles;

namespace TeslaStarter.Infrastructure.Persistence.Repositories;

public class VehicleRepository(TeslaStarterDbContext context) : IVehicleRepository
{
    public async Task<Vehicle?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        if (id is VehicleId vehicleId)
        {
            return await GetByIdAsync(vehicleId, cancellationToken);
        }
        return null;
    }

    public async Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default)
    {
        return await context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByTeslaAccountIdAsync(TeslaAccountId teslaAccountId, CancellationToken cancellationToken = default)
    {
        return await context.Vehicles
            .Where(v => v.TeslaAccountId == teslaAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(VehicleId id, CancellationToken cancellationToken = default)
    {
        return await context.Vehicles
            .AnyAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vehicle?> GetByVehicleIdentifierAsync(string vehicleIdentifier, CancellationToken cancellationToken = default)
    {
        return await context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleIdentifier == vehicleIdentifier, cancellationToken);
    }

    public void Add(Vehicle vehicle)
    {
        context.Vehicles.Add(vehicle);
    }

    public void Update(Vehicle vehicle)
    {
        context.Vehicles.Update(vehicle);
    }

    public void Remove(Vehicle vehicle)
    {
        context.Vehicles.Remove(vehicle);
    }
}
