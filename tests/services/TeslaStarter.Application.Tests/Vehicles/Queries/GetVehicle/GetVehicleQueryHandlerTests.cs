using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles.Queries.GetVehicle;

public sealed class GetVehicleQueryHandlerTests : ApplicationTestBase
{
    private readonly GetVehicleQueryHandler _handler;

    public GetVehicleQueryHandlerTests()
    {
        _handler = new GetVehicleQueryHandler(
            VehicleRepositoryMock.Object,
            Mapper);
    }

    [Fact]
    public async Task Handle_VehicleExists_ReturnsVehicleDto()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "My Tesla");
        GetVehicleQuery query = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(vehicle.Id.Value);
        result.TeslaAccountId.Should().Be("tesla123");
        result.VehicleIdentifier.Should().Be("VIN123");
        result.DisplayName.Should().Be("My Tesla");
        result.IsActive.Should().BeTrue();

        VehicleRepositoryMock.Verify(x => x.GetByIdAsync(
            It.IsAny<VehicleId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        GetVehicleQuery query = new()
        {
            VehicleId = Guid.NewGuid()
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        VehicleRepositoryMock.Verify(x => x.GetByIdAsync(
            It.IsAny<VehicleId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveVehicle_ReturnsVehicleDtoWithInactiveStatus()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.Deactivate(); // Make inactive

        GetVehicleQuery query = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_VehicleWithoutDisplayName_ReturnsDtoWithNullDisplayName()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", null);
        GetVehicleQuery query = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_VehicleWithLastSyncedAt_ReturnsDtoWithLastSyncedAt()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle();
        vehicle.RecordSync(); // Set LastSyncedAt

        GetVehicleQuery query = new()
        {
            VehicleId = vehicle.Id.Value
        };

        VehicleRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<VehicleId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        VehicleDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.LastSyncedAt.Should().NotBeNull();
        result.LastSyncedAt.Should().Be(vehicle.LastSyncedAt);
    }
}
