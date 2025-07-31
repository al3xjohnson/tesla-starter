using Common.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeslaStarter.Domain.Vehicles;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence.Repositories;

public sealed class VehicleRepositoryTests : IDisposable
{
    private readonly TeslaStarterDbContext _context;
    private readonly VehicleRepository _repository;

    public VehicleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TeslaStarterDbContext(options);
        _repository = new VehicleRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Add_ShouldAddVehicleToContext()
    {
        var vehicle = CreateVehicle();

        _repository.Add(vehicle);
        await _context.SaveChangesAsync();

        var savedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
        savedVehicle.Should().NotBeNull();
        savedVehicle!.VehicleIdentifier.Should().Be(vehicle.VehicleIdentifier);
    }

    [Fact]
    public async Task Update_ShouldUpdateVehicleInContext()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        vehicle.UpdateDisplayName("New Display Name");
        _repository.Update(vehicle);
        await _context.SaveChangesAsync();

        var updatedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
        updatedVehicle!.DisplayName.Should().Be("New Display Name");
    }

    [Fact]
    public async Task Remove_ShouldRemoveVehicleFromContext()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        _repository.Remove(vehicle);
        await _context.SaveChangesAsync();

        var removedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
        removedVehicle.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Generic_WithVehicleId_ShouldReturnVehicle()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync<VehicleId>(vehicle.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(vehicle.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Generic_WithNonVehicleId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync<string>("not-a-vehicleid");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithVehicleId_ShouldReturnVehicle()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(vehicle.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(vehicle.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(VehicleId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTeslaAccountIdAsync_ShouldReturnVehiclesForAccount()
    {
        var teslaAccountId = TeslaAccountId.Create("tesla123");
        var vehicle1 = CreateVehicle(teslaAccountId);
        var vehicle2 = CreateVehicle(teslaAccountId);
        var vehicle3 = CreateVehicle(TeslaAccountId.Create("other"));

        _context.Vehicles.AddRange(vehicle1, vehicle2, vehicle3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByTeslaAccountIdAsync(teslaAccountId);

        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Id == vehicle1.Id);
        result.Should().Contain(v => v.Id == vehicle2.Id);
    }

    [Fact]
    public async Task GetByTeslaAccountIdAsync_WithNoVehicles_ShouldReturnEmptyList()
    {
        var result = await _repository.GetByTeslaAccountIdAsync(TeslaAccountId.Create("nonexistent"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsAsync(vehicle.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var result = await _repository.ExistsAsync(VehicleId.New());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByVehicleIdentifierAsync_ShouldReturnVehicle()
    {
        var vehicle = CreateVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByVehicleIdentifierAsync(vehicle.VehicleIdentifier);

        result.Should().NotBeNull();
        result!.VehicleIdentifier.Should().Be(vehicle.VehicleIdentifier);
    }

    [Fact]
    public async Task GetByVehicleIdentifierAsync_WithNonExistentIdentifier_ShouldReturnNull()
    {
        var result = await _repository.GetByVehicleIdentifierAsync("nonexistent");

        result.Should().BeNull();
    }

    private static Vehicle CreateVehicle(TeslaAccountId? teslaAccountId = null)
    {
        var id = VehicleId.New();
        var accountId = teslaAccountId ?? TeslaAccountId.Create("tesla123");
        var vehicle = Vehicle.Link(accountId, Guid.NewGuid().ToString(), "Model S");

        return vehicle;
    }
}
