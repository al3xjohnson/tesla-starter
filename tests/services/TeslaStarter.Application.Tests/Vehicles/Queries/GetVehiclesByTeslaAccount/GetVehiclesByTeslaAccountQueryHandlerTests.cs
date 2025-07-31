using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Vehicles.Queries.GetVehiclesByTeslaAccount;

public sealed class GetVehiclesByTeslaAccountQueryHandlerTests : ApplicationTestBase
{
    private readonly GetVehiclesByTeslaAccountQueryHandler _handler;

    public GetVehiclesByTeslaAccountQueryHandlerTests()
    {
        _handler = new GetVehiclesByTeslaAccountQueryHandler(
            VehicleRepositoryMock.Object,
            Mapper);
    }

    [Fact]
    public async Task Handle_VehiclesExist_ReturnsVehicleDtoList()
    {
        // Arrange
        List<Vehicle> vehicles =
        [
            CreateTestVehicle("tesla123", "VIN1", "Tesla 1"),
            CreateTestVehicle("tesla123", "VIN2", "Tesla 2"),
            CreateTestVehicle("tesla123", "VIN3", null)
        ];

        GetVehiclesByTeslaAccountQuery query = new()
        {
            TeslaAccountId = "tesla123"
        };

        VehicleRepositoryMock.Setup(x => x.GetByTeslaAccountIdAsync(
                It.IsAny<TeslaAccountId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        // Act
        IReadOnlyList<VehicleDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].VehicleIdentifier.Should().Be("VIN1");
        result[0].DisplayName.Should().Be("Tesla 1");
        result[1].VehicleIdentifier.Should().Be("VIN2");
        result[1].DisplayName.Should().Be("Tesla 2");
        result[2].VehicleIdentifier.Should().Be("VIN3");
        result[2].DisplayName.Should().BeNull();

        VehicleRepositoryMock.Verify(x => x.GetByTeslaAccountIdAsync(
            It.Is<TeslaAccountId>(t => t.Value == "tesla123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoVehicles_ReturnsEmptyList()
    {
        // Arrange
        GetVehiclesByTeslaAccountQuery query = new()
        {
            TeslaAccountId = "tesla123"
        };

        VehicleRepositoryMock.Setup(x => x.GetByTeslaAccountIdAsync(
                It.IsAny<TeslaAccountId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vehicle>());

        // Act
        IReadOnlyList<VehicleDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        VehicleRepositoryMock.Verify(x => x.GetByTeslaAccountIdAsync(
            It.IsAny<TeslaAccountId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MixedActiveAndInactiveVehicles_ReturnsAllVehicles()
    {
        // Arrange
        Vehicle activeVehicle = CreateTestVehicle("tesla123", "VIN1", "Active");
        Vehicle inactiveVehicle = CreateTestVehicle("tesla123", "VIN2", "Inactive");
        inactiveVehicle.Deactivate();

        List<Vehicle> vehicles = [activeVehicle, inactiveVehicle];

        GetVehiclesByTeslaAccountQuery query = new()
        {
            TeslaAccountId = "tesla123"
        };

        VehicleRepositoryMock.Setup(x => x.GetByTeslaAccountIdAsync(
                It.IsAny<TeslaAccountId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        // Act
        IReadOnlyList<VehicleDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].IsActive.Should().BeTrue();
        result[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyTeslaAccountId_ThrowsArgumentException()
    {
        // Arrange
        GetVehiclesByTeslaAccountQuery query = new()
        {
            TeslaAccountId = ""
        };

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Contain("Tesla account ID cannot be empty");

        VehicleRepositoryMock.Verify(x => x.GetByTeslaAccountIdAsync(
            It.IsAny<TeslaAccountId>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SingleVehicle_ReturnsListWithOneItem()
    {
        // Arrange
        Vehicle vehicle = CreateTestVehicle("tesla123", "VIN123", "My Tesla");
        List<Vehicle> vehicles = [vehicle];

        GetVehiclesByTeslaAccountQuery query = new()
        {
            TeslaAccountId = "tesla123"
        };

        VehicleRepositoryMock.Setup(x => x.GetByTeslaAccountIdAsync(
                It.IsAny<TeslaAccountId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        // Act
        IReadOnlyList<VehicleDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].TeslaAccountId.Should().Be("tesla123");
        result[0].VehicleIdentifier.Should().Be("VIN123");
        result[0].DisplayName.Should().Be("My Tesla");
    }
}
