using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles.Commands.UnlinkVehicle;

public sealed class UnlinkVehicleCommandHandlerTests : ApplicationTestBase
{
    private readonly UnlinkVehicleCommandHandler _handler;
    private readonly Mock<ILogger<UnlinkVehicleCommandHandler>> _loggerMock;

    public UnlinkVehicleCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<UnlinkVehicleCommandHandler>();
        _handler = new UnlinkVehicleCommandHandler(
            VehicleRepositoryMock.Object,
            UnitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UnlinksVehicleSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        UnlinkVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        VehicleRepositoryMock.Verify(x => x.Update(
            It.Is<Vehicle>(v => !v.IsActive)), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unlinked vehicle")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        UnlinkVehicleCommand command = new()
        {
            VehicleId = Guid.NewGuid()
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
    public async Task Handle_AlreadyUnlinkedVehicle_StillProcessesSuccessfully()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.Deactivate(); // Already deactivated

        UnlinkVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Still processes the update even if already unlinked
        VehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unlinked vehicle")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenThrough()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        UnlinkVehicleCommand command = new()
        {
            VehicleId = vehicle.Id.Value
        };
        CancellationToken cancellationToken = new CancellationToken();

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                cancellationToken))
            .ReturnsAsync(vehicle);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        VehicleRepositoryMock.Verify(x => x.GetByIdAsync(
            It.IsAny<VehicleId>(),
            cancellationToken), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
