namespace TeslaStarter.Domain.Tests.Vehicles;

public class VehicleIdTests
{
    [Fact]
    public void New_CreatesUniqueVehicleIds()
    {
        // Act
        VehicleId vehicleId1 = VehicleId.New();
        VehicleId vehicleId2 = VehicleId.New();

        // Assert
        vehicleId1.Should().NotBe(vehicleId2);
        vehicleId1.Value.Should().NotBe(Guid.Empty);
        vehicleId2.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Empty_ReturnsVehicleIdWithEmptyGuid()
    {
        // Act
        VehicleId emptyVehicleId = VehicleId.Empty;

        // Assert
        emptyVehicleId.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        VehicleId vehicleId = new VehicleId(guid);

        // Act
        string result = vehicleId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Constructor_WithGuid_CreatesVehicleId()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act
        VehicleId vehicleId = new VehicleId(guid);

        // Assert
        vehicleId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_WithSameValue_ReturnsTrue()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        VehicleId vehicleId1 = new VehicleId(guid);
        VehicleId vehicleId2 = new VehicleId(guid);

        // Assert
        vehicleId1.Should().Be(vehicleId2);
        (vehicleId1 == vehicleId2).Should().BeTrue();
        vehicleId1.GetHashCode().Should().Be(vehicleId2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        VehicleId vehicleId1 = new VehicleId(Guid.NewGuid());
        VehicleId vehicleId2 = new VehicleId(Guid.NewGuid());

        // Assert
        vehicleId1.Should().NotBe(vehicleId2);
        (vehicleId1 != vehicleId2).Should().BeTrue();
    }

    [Fact]
    public void WithOperator_CreatesNewInstanceWithSameValue()
    {
        // Arrange
        VehicleId vehicleId = new VehicleId(Guid.NewGuid());

        // Act
        VehicleId copy = vehicleId with { };

        // Assert
        copy.Should().Be(vehicleId);
        copy.Should().NotBeSameAs(vehicleId);
    }
}
