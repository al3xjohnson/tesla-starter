using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateVehicleCommandHandler _handler;
    private readonly Mock<ILogger<UpdateVehicleCommandHandler>> _loggerMock;

    public UpdateVehicleCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<UpdateVehicleCommandHandler>();
        _handler = new UpdateVehicleCommandHandler(
            VehicleRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesVehicleSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        UpdateVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value,
            DisplayName = "Updated Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(command.VehicleId);
        result.DisplayName.Should().Be(command.DisplayName);

        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated vehicle")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        UpdateVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid(),
            DisplayName = "Updated Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullDisplayName_UpdatesVehicleSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "Original Name");
        UpdateVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value,
            DisplayName = null
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().BeNull();

        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SameDisplayName_StillUpdatesSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "My Tesla");
        UpdateVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value,
            DisplayName = "My Tesla" // Same name
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(command.DisplayName);

        // Still calls update even if the value is the same
        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveVehicle_StillUpdatesSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.Deactivate(); // Make it inactive

        UpdateVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value,
            DisplayName = "Updated Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(command.DisplayName);
        result.IsActive.Should().BeFalse(); // Still inactive

        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
