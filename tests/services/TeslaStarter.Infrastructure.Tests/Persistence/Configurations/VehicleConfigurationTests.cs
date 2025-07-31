using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeslaStarter.Domain.Vehicles;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Tests.TestHelpers;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence.Configurations;

public sealed class VehicleConfigurationTests : IDisposable
{
    private readonly TeslaStarterDbContext _context;

    public VehicleConfigurationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
    }

    [Fact]
    public void VehicleConfiguration_Should_Configure_Table_Name()
    {
        // Arrange & Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = _context.Model.FindEntityType(typeof(Vehicle));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("vehicles");
    }

    [Fact]
    public void VehicleConfiguration_Should_Configure_Primary_Key()
    {
        // Arrange & Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = _context.Model.FindEntityType(typeof(Vehicle));
        Microsoft.EntityFrameworkCore.Metadata.IKey? primaryKey = entityType!.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void VehicleConfiguration_Should_Configure_Properties()
    {
        // Arrange & Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = _context.Model.FindEntityType(typeof(Vehicle));

        // Assert
        Microsoft.EntityFrameworkCore.Metadata.IProperty? idProperty = entityType!.FindProperty("Id");
        idProperty.Should().NotBeNull();
        idProperty!.IsNullable.Should().BeFalse();

        Microsoft.EntityFrameworkCore.Metadata.IProperty? teslaAccountIdProperty = entityType!.FindProperty("TeslaAccountId");
        teslaAccountIdProperty.Should().NotBeNull();
        teslaAccountIdProperty!.IsNullable.Should().BeFalse();
        teslaAccountIdProperty.GetMaxLength().Should().Be(100);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? vehicleIdentifierProperty = entityType!.FindProperty("VehicleIdentifier");
        vehicleIdentifierProperty.Should().NotBeNull();
        vehicleIdentifierProperty!.IsNullable.Should().BeFalse();
        vehicleIdentifierProperty.GetMaxLength().Should().Be(100);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? displayNameProperty = entityType!.FindProperty("DisplayName");
        displayNameProperty.Should().NotBeNull();
        displayNameProperty!.GetMaxLength().Should().Be(100);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? linkedAtProperty = entityType!.FindProperty("LinkedAt");
        linkedAtProperty.Should().NotBeNull();
        linkedAtProperty!.IsNullable.Should().BeFalse();

        Microsoft.EntityFrameworkCore.Metadata.IProperty? lastSyncedAtProperty = entityType!.FindProperty("LastSyncedAt");
        lastSyncedAtProperty.Should().NotBeNull();
        lastSyncedAtProperty!.IsNullable.Should().BeTrue();

        Microsoft.EntityFrameworkCore.Metadata.IProperty? isActiveProperty = entityType!.FindProperty("IsActive");
        isActiveProperty.Should().NotBeNull();
        isActiveProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VehicleConfiguration_Should_Configure_Indexes()
    {
        // Arrange & Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = _context.Model.FindEntityType(typeof(Vehicle));
        List<Microsoft.EntityFrameworkCore.Metadata.IIndex> indexes = entityType!.GetIndexes().ToList();

        // Assert
        indexes.Should().NotBeEmpty();
        indexes.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public void VehicleConfiguration_Should_Ignore_DomainEvents()
    {
        // Arrange & Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = _context.Model.FindEntityType(typeof(Vehicle));
        Microsoft.EntityFrameworkCore.Metadata.IProperty? domainEventsProperty = entityType!.FindProperty("DomainEvents");

        // Assert
        domainEventsProperty.Should().BeNull();
    }

    [Fact]
    public async Task VehicleConfiguration_Should_Save_And_Retrieve_Vehicle()
    {
        // Arrange
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        Vehicle vehicle = Vehicle.Link(teslaAccountId, "VIN123456789", "My Tesla");

        // Act
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        Vehicle savedVehicle = await _context.Vehicles.FirstAsync();

        // Assert
        savedVehicle.Id.Should().Be(vehicle.Id);
        savedVehicle.TeslaAccountId.Value.Should().Be("tesla123");
        savedVehicle.VehicleIdentifier.Should().Be("VIN123456789");
        savedVehicle.DisplayName.Should().Be("My Tesla");
        savedVehicle.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task VehicleConfiguration_Should_Allow_Multiple_Vehicles_Same_VIN()
    {
        // Arrange - Two different Tesla accounts can have the same vehicle
        Vehicle vehicle1 = Vehicle.Link(
            TeslaAccountId.Create("tesla123"),
            "VIN123456789",
            "User1's Tesla");

        Vehicle vehicle2 = Vehicle.Link(
            TeslaAccountId.Create("tesla456"),
            "VIN123456789",
            "User2's Tesla");

        // Act
        _context.Vehicles.AddRange(vehicle1, vehicle2);
        await _context.SaveChangesAsync();

        // Assert
        List<Vehicle> vehicles = await _context.Vehicles
            .Where(v => v.VehicleIdentifier == "VIN123456789")
            .ToListAsync();

        vehicles.Should().HaveCount(2);
        vehicles.Should().Contain(v => v.TeslaAccountId.Value == "tesla123");
        vehicles.Should().Contain(v => v.TeslaAccountId.Value == "tesla456");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
