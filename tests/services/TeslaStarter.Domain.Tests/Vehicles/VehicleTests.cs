using Common.Domain.Events;

namespace TeslaStarter.Domain.Tests.Vehicles;

public class VehicleTests
{
    [Fact]
    public void Link_WithValidData_CreatesVehicle()
    {
        // Arrange
        TeslaAccountId teslaAccountId = TeslaAccountId.Create("tesla123");
        string vehicleIdentifier = "5YJ3E1EA1JF00001";
        string displayName = "My Tesla";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        Vehicle vehicle = Vehicle.Link(teslaAccountId, vehicleIdentifier, displayName);

        // Assert
        vehicle.Should().NotBeNull();
        vehicle.Id.Should().NotBe(VehicleId.Empty);
        vehicle.TeslaAccountId.Should().Be(teslaAccountId);
        vehicle.VehicleIdentifier.Should().Be(vehicleIdentifier);
        vehicle.DisplayName.Should().Be(displayName);
        vehicle.LinkedAt.Should().BeOnOrAfter(beforeCreation);
        vehicle.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        vehicle.LastSyncedAt.Should().BeNull();
        vehicle.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Link_WithoutDisplayName_CreatesVehicleWithNullDisplayName()
    {
        // Act
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");

        // Assert
        vehicle.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Link_WithWhitespaceInIdentifier_TrimsIdentifier()
    {
        // Act
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "  5YJ3E1EA1JF00001  ", "  My Tesla  ");

        // Assert
        vehicle.VehicleIdentifier.Should().Be("5YJ3E1EA1JF00001");
        vehicle.DisplayName.Should().Be("My Tesla");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Link_WithInvalidVehicleIdentifier_ThrowsArgumentException(string invalidIdentifier)
    {
        // Act & Assert
        Action act = () => Vehicle.Link(TeslaAccountId.Create("tesla123"), invalidIdentifier);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Vehicle identifier cannot be empty*")
            .And.ParamName.Should().Be("vehicleIdentifier");
    }

    [Fact]
    public void Link_RaisesVehicleLinkedDomainEvent()
    {
        // Act
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001", "My Tesla");

        // Assert
        List<IDomainEvent> domainEvents = vehicle.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        VehicleLinkedDomainEvent linkedEvent = domainEvents[0].Should().BeOfType<VehicleLinkedDomainEvent>().Subject;
        linkedEvent.VehicleId.Should().Be(vehicle.Id);
        linkedEvent.TeslaAccountId.Should().Be(vehicle.TeslaAccountId);
        linkedEvent.VehicleIdentifier.Should().Be(vehicle.VehicleIdentifier);
        linkedEvent.DisplayName.Should().Be(vehicle.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_ChangesDisplayName()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001", "Old Name");
        vehicle.ClearDomainEvents();

        // Act
        vehicle.UpdateDisplayName("New Name");

        // Assert
        vehicle.DisplayName.Should().Be("New Name");
        vehicle.GetDomainEvents().Should().BeEmpty(); // No event for display name change
    }

    [Fact]
    public void UpdateDisplayName_WithNull_SetsToNull()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001", "Name");

        // Act
        vehicle.UpdateDisplayName(null);

        // Assert
        vehicle.DisplayName.Should().BeNull();
    }

    [Fact]
    public void UpdateDisplayName_WithWhitespace_TrimsValue()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");

        // Act
        vehicle.UpdateDisplayName("  New Name  ");

        // Assert
        vehicle.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void RecordSync_UpdatesLastSyncedAt()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");
        DateTime beforeSync = DateTime.UtcNow;

        // Act
        vehicle.RecordSync();

        // Assert
        vehicle.LastSyncedAt.Should().NotBeNull();
        vehicle.LastSyncedAt.Should().BeOnOrAfter(beforeSync);
        vehicle.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordSync_MultipleTimes_UpdatesEachTime()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");
        vehicle.RecordSync();
        DateTime? firstSync = vehicle.LastSyncedAt;
        Thread.Sleep(10);

        // Act
        vehicle.RecordSync();

        // Assert
        vehicle.LastSyncedAt.Should().BeAfter(firstSync!.Value);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");

        // Act
        vehicle.Deactivate();

        // Assert
        vehicle.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");
        vehicle.Deactivate();

        // Act & Assert
        Action act = () => vehicle.Deactivate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Vehicle is already inactive");
    }

    [Fact]
    public void Reactivate_SetsIsActiveToTrue()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");
        vehicle.Deactivate();

        // Act
        vehicle.Reactivate();

        // Assert
        vehicle.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_WhenAlreadyActive_ThrowsInvalidOperationException()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "5YJ3E1EA1JF00001");

        // Act & Assert
        Action act = () => vehicle.Reactivate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Vehicle is already active");
    }

    [Fact]
    public void MultipleUsersCanHaveSameVehicleIdentifier()
    {
        // Arrange
        TeslaAccountId teslaAccount1 = TeslaAccountId.Create("tesla123");
        TeslaAccountId teslaAccount2 = TeslaAccountId.Create("tesla456");
        string vehicleIdentifier = "5YJ3E1EA1JF00001"; // Same VIN

        // Act
        Vehicle vehicle1 = Vehicle.Link(teslaAccount1, vehicleIdentifier, "User1's Tesla");
        Vehicle vehicle2 = Vehicle.Link(teslaAccount2, vehicleIdentifier, "User2's Tesla");

        // Assert
        vehicle1.Id.Should().NotBe(vehicle2.Id); // Different instances
        vehicle1.TeslaAccountId.Should().NotBe(vehicle2.TeslaAccountId); // Different owners
        vehicle1.VehicleIdentifier.Should().Be(vehicle2.VehicleIdentifier); // Same physical vehicle
        vehicle1.DisplayName.Should().NotBe(vehicle2.DisplayName); // Different names
    }
}
