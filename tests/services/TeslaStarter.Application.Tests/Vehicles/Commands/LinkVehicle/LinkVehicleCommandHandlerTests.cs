using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles.Commands.LinkVehicle;

public sealed class LinkVehicleCommandHandlerTests : ApplicationTestBase
{
    private readonly LinkVehicleCommandHandler _handler;
    private readonly Mock<ILogger<LinkVehicleCommandHandler>> _loggerMock;

    public LinkVehicleCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<LinkVehicleCommandHandler>();
        _handler = new LinkVehicleCommandHandler(
            VehicleRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_LinksVehicleSuccessfully()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByVehicleIdentifierAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TeslaAccountId.Should().Be(command.TeslaAccountId);
        result.VehicleIdentifier.Should().Be(command.VehicleIdentifier);
        result.DisplayName.Should().Be(command.DisplayName);
        result.IsActive.Should().BeTrue();

        VehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Linked vehicle")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_VehicleAlreadyLinkedToSameTeslaAccount_ThrowsValidationException()
    {
        // Arrange
        Vehicle existingVehicle = CreateTestVehicle("tesla123", "VIN123");
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByVehicleIdentifierAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("VehicleIdentifier");
        exception.Errors["VehicleIdentifier"].Should().Contain("This vehicle is already linked to this Tesla account.");

        VehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_VehicleLinkedToDifferentTeslaAccount_LinksSuccessfully()
    {
        // Arrange
        Vehicle existingVehicle = CreateTestVehicle("tesla456", "VIN123");
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByVehicleIdentifierAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TeslaAccountId.Should().Be(command.TeslaAccountId);
        result.VehicleIdentifier.Should().Be(command.VehicleIdentifier);

        VehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutDisplayName_LinksVehicleSuccessfully()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = null
        };

        VehicleRepositoryMock.Setup(x => x.GetByVehicleIdentifierAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        VehicleDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().BeNull();
        result.IsActive.Should().BeTrue();

        VehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveExistingVehicleWithSameTeslaAccount_StillThrowsValidationException()
    {
        // Arrange
        Vehicle existingVehicle = CreateTestVehicle("tesla123", "VIN123");
        existingVehicle.Deactivate(); // Make it inactive

        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        VehicleRepositoryMock.Setup(x => x.GetByVehicleIdentifierAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("VehicleIdentifier");
        exception.Errors["VehicleIdentifier"].Should().Contain("This vehicle is already linked to this Tesla account.");

        VehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
