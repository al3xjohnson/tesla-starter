using System.Diagnostics.CodeAnalysis;
using Common.Domain.Persistence;
using Common.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TeslaStarter.Application.Common.Exceptions;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Application.Vehicles.Commands.SyncUserVehicles;
using TeslaStarter.Domain.Users;
using TeslaStarter.Domain.Vehicles;
using Xunit;

namespace TeslaStarter.Application.Tests.Vehicles.Commands.SyncUserVehicles;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class SyncUserVehiclesCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<ITeslaApiService> _teslaApiServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SyncUserVehiclesCommandHandler>> _loggerMock;
    private readonly SyncUserVehiclesCommandHandler _handler;

    public SyncUserVehiclesCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _teslaApiServiceMock = new Mock<ITeslaApiService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SyncUserVehiclesCommandHandler>>();

        _handler = new SyncUserVehiclesCommandHandler(
            _userRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _teslaApiServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        SyncUserVehiclesCommand command = new() { ExternalId = "nonexistent" };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Entity \"User\" (nonexistent) was not found.");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoTeslaAccount_ReturnsZero()
    {
        // Arrange
        string externalId = "descope123";
        User user = User.Create(externalId, "test@example.com", "Test User");

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);
        _teslaApiServiceMock.Verify(x => x.GetVehiclesAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoVehiclesFound_ReturnsZero()
    {
        // Arrange
        string externalId = "descope123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount("tesla123");
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto>());

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);
        _vehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNewVehicleFound_AddsVehicle()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        TeslaVehicleDto teslaVehicle = new()
        {
            Id = "12345",
            Vin = "5YJ3E1EA1JF000001",
            DisplayName = "My Tesla",
            State = "online"
        };

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto> { teslaVehicle });

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync(teslaVehicle.Vin, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        _vehicleRepositoryMock.Verify(x => x.Add(It.Is<Vehicle>(v =>
            v.TeslaAccountId == teslaAccountId &&
            v.VehicleIdentifier == teslaVehicle.Vin &&
            v.DisplayName == teslaVehicle.DisplayName)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVehicleWithEmptyDisplayName_AddsVehicleWithNullDisplayName()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        TeslaVehicleDto teslaVehicle = new()
        {
            Id = "12345",
            Vin = "5YJ3E1EA1JF000001",
            DisplayName = "",
            State = "online"
        };

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto> { teslaVehicle });

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync(teslaVehicle.Vin, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        _vehicleRepositoryMock.Verify(x => x.Add(It.Is<Vehicle>(v =>
            v.DisplayName == null)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExistingVehicleForSameUser_UpdatesVehicle()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        Vehicle existingVehicle = Vehicle.Link(TeslaAccountId.Create(teslaAccountId), "5YJ3E1EA1JF000001", "Old Name");

        TeslaVehicleDto teslaVehicle = new()
        {
            Id = "12345",
            Vin = "5YJ3E1EA1JF000001",
            DisplayName = "New Name",
            State = "online"
        };

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto> { teslaVehicle });

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync(teslaVehicle.Vin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        existingVehicle.DisplayName.Should().Be("New Name");
        _vehicleRepositoryMock.Verify(x => x.Update(existingVehicle), Times.Once);
        _vehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExistingVehicleForDifferentUser_DoesNotUpdateVehicle()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        // Vehicle belongs to different Tesla account
        Vehicle existingVehicle = Vehicle.Link(TeslaAccountId.Create("different-tesla-account"), "5YJ3E1EA1JF000001", "Other User's Tesla");

        TeslaVehicleDto teslaVehicle = new()
        {
            Id = "12345",
            Vin = "5YJ3E1EA1JF000001",
            DisplayName = "My Tesla",
            State = "online"
        };

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto> { teslaVehicle });

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync(teslaVehicle.Vin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);
        existingVehicle.DisplayName.Should().Be("Other User's Tesla"); // Not updated
        _vehicleRepositoryMock.Verify(x => x.Update(It.IsAny<Vehicle>()), Times.Never);
        _vehicleRepositoryMock.Verify(x => x.Add(It.IsAny<Vehicle>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleVehicles_SyncsAllVehicles()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        Vehicle existingVehicle = Vehicle.Link(TeslaAccountId.Create(teslaAccountId), "5YJ3E1EA1JF000001", "Old Name");

        List<TeslaVehicleDto> teslaVehicles =
        [
            new() { Id = "12345", Vin = "5YJ3E1EA1JF000001", DisplayName = "Updated Tesla", State = "online" },
            new() { Id = "67890", Vin = "5YJ3E1EA1JF000002", DisplayName = "New Tesla", State = "asleep" },
            new() { Id = "11111", Vin = "5YJ3E1EA1JF000003", DisplayName = "", State = "online" }
        ];

        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(teslaVehicles);

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync("5YJ3E1EA1JF000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync("5YJ3E1EA1JF000002", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        _vehicleRepositoryMock
            .Setup(x => x.GetByVehicleIdentifierAsync("5YJ3E1EA1JF000003", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        int result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(3);

        // Verify existing vehicle was updated
        existingVehicle.DisplayName.Should().Be("Updated Tesla");
        _vehicleRepositoryMock.Verify(x => x.Update(existingVehicle), Times.Once);

        // Verify new vehicles were added
        _vehicleRepositoryMock.Verify(x => x.Add(It.Is<Vehicle>(v =>
            v.VehicleIdentifier == "5YJ3E1EA1JF000002" && v.DisplayName == "New Tesla")), Times.Once);

        _vehicleRepositoryMock.Verify(x => x.Add(It.Is<Vehicle>(v =>
            v.VehicleIdentifier == "5YJ3E1EA1JF000003" && v.DisplayName == null)), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToAllMethods()
    {
        // Arrange
        string externalId = "descope123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount("tesla123");
        user.UpdateTeslaTokens("access-token", "refresh-token", DateTime.UtcNow.AddHours(8));

        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        SyncUserVehiclesCommand command = new() { ExternalId = externalId };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teslaApiServiceMock
            .Setup(x => x.GetVehiclesAsync("access-token"))
            .ReturnsAsync(new List<TeslaVehicleDto>());

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            cancellationToken), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
