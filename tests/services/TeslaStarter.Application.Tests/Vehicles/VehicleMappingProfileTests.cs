using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles;

public sealed class VehicleMappingProfileTests : ApplicationTestBase
{
    [Fact]
    public void VehicleMappingProfile_ConfigurationIsValid()
    {
        // Act & Assert
        Mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Vehicle_To_VehicleDto_MapsAllProperties()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "My Tesla");

        // Act
        VehicleDto dto = Mapper.Map<VehicleDto>(vehicle);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(vehicle.Id.Value);
        dto.TeslaAccountId.Should().Be("tesla123");
        dto.VehicleIdentifier.Should().Be("VIN123");
        dto.DisplayName.Should().Be("My Tesla");
        dto.LinkedAt.Should().Be(vehicle.LinkedAt);
        dto.LastSyncedAt.Should().BeNull();
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Map_VehicleWithNullDisplayName_To_VehicleDto_MapsNullDisplayName()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", null);

        // Act
        VehicleDto dto = Mapper.Map<VehicleDto>(vehicle);

        // Assert
        dto.Should().NotBeNull();
        dto.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Map_InactiveVehicle_To_VehicleDto_MapsIsActiveFalse()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.Deactivate();

        // Act
        VehicleDto dto = Mapper.Map<VehicleDto>(vehicle);

        // Assert
        dto.Should().NotBeNull();
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Map_VehicleWithLastSyncedAt_To_VehicleDto_MapsLastSyncedAt()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.RecordSync();

        // Act
        VehicleDto dto = Mapper.Map<VehicleDto>(vehicle);

        // Assert
        dto.Should().NotBeNull();
        dto.LastSyncedAt.Should().NotBeNull();
        dto.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Map_MultipleVehicles_To_VehicleDtoList_MapsAllVehicles()
    {
        // Arrange
        List<Vehicle> vehicles =
        [
            CreateTestVehicle("tesla1", "VIN1", "Tesla 1"),
            CreateTestVehicle("tesla2", "VIN2", "Tesla 2"),
            CreateTestVehicle("tesla3", "VIN3", null)
        ];

        // Act
        IReadOnlyList<VehicleDto> dtos = Mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().HaveCount(3);

        dtos[0].TeslaAccountId.Should().Be("tesla1");
        dtos[0].VehicleIdentifier.Should().Be("VIN1");
        dtos[0].DisplayName.Should().Be("Tesla 1");

        dtos[1].TeslaAccountId.Should().Be("tesla2");
        dtos[1].VehicleIdentifier.Should().Be("VIN2");
        dtos[1].DisplayName.Should().Be("Tesla 2");

        dtos[2].TeslaAccountId.Should().Be("tesla3");
        dtos[2].VehicleIdentifier.Should().Be("VIN3");
        dtos[2].DisplayName.Should().BeNull();
    }

    [Fact]
    public void Map_EmptyVehicleList_To_VehicleDtoList_ReturnsEmptyList()
    {
        // Arrange
        List<Vehicle> vehicles = [];

        // Act
        IReadOnlyList<VehicleDto> dtos = Mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void Map_VehicleAfterUpdateDisplayName_To_VehicleDto_MapsUpdatedName()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "Original Name");
        vehicle.UpdateDisplayName("Updated Name");

        // Act
        VehicleDto dto = Mapper.Map<VehicleDto>(vehicle);

        // Assert
        dto.Should().NotBeNull();
        dto.DisplayName.Should().Be("Updated Name");
    }
}
