namespace TeslaStarter.Domain.Tests.Vehicles.Events;

public class VehicleLinkedDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        VehicleId vehicleId = VehicleId.New();
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        string vehicleIdentifier = "5YJ3E1EA1JF00001";
        string displayName = "My Tesla";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        VehicleLinkedDomainEvent @event = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, displayName);

        // Assert
        @event.VehicleId.Should().Be(vehicleId);
        @event.TeslaAccountId.Should().Be(teslaAccountId);
        @event.VehicleIdentifier.Should().Be(vehicleIdentifier);
        @event.DisplayName.Should().Be(displayName);
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullDisplayName_InitializesCorrectly()
    {
        // Arrange
        VehicleId vehicleId = VehicleId.New();
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        string vehicleIdentifier = "5YJ3E1EA1JF00001";

        // Act
        VehicleLinkedDomainEvent @event = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, null);

        // Assert
        @event.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Constructor_EachInstanceHasUniqueId()
    {
        // Arrange
        VehicleId vehicleId = VehicleId.New();
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        string vehicleIdentifier = "5YJ3E1EA1JF00001";

        // Act
        VehicleLinkedDomainEvent event1 = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, null);
        VehicleLinkedDomainEvent event2 = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, null);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void RecordEquality_WithSameData_PropertiesMatch()
    {
        // Arrange
        VehicleId vehicleId = VehicleId.New();
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        string vehicleIdentifier = "5YJ3E1EA1JF00001";
        string displayName = "My Tesla";
        VehicleLinkedDomainEvent event1 = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, displayName);
        VehicleLinkedDomainEvent event2 = new VehicleLinkedDomainEvent(vehicleId, teslaAccountId, vehicleIdentifier, displayName);

        // Assert
        event1.VehicleId.Should().Be(event2.VehicleId);
        event1.TeslaAccountId.Should().Be(event2.TeslaAccountId);
        event1.VehicleIdentifier.Should().Be(event2.VehicleIdentifier);
        event1.DisplayName.Should().Be(event2.DisplayName);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ReturnsFalse()
    {
        // Arrange
        VehicleLinkedDomainEvent event1 = new VehicleLinkedDomainEvent(VehicleId.New(), TeslaAccountId.Create("tesla123"), "VIN1", "Tesla1");
        VehicleLinkedDomainEvent event2 = new VehicleLinkedDomainEvent(VehicleId.New(), TeslaAccountId.Create("tesla456"), "VIN2", "Tesla2");

        // Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        VehicleLinkedDomainEvent @event = new VehicleLinkedDomainEvent(VehicleId.New(), TeslaAccountId.Create("tesla123"), "VIN", null);

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
