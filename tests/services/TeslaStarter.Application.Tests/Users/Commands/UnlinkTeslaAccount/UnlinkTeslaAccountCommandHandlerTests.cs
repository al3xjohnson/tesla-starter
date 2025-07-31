using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Commands.UnlinkTeslaAccount;

public sealed class UnlinkTeslaAccountCommandHandlerTests : ApplicationTestBase
{
    private readonly UnlinkTeslaAccountCommandHandler _handler;
    private readonly Mock<ILogger<UnlinkTeslaAccountCommandHandler>> _loggerMock;

    public UnlinkTeslaAccountCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<UnlinkTeslaAccountCommandHandler>();
        _handler = new UnlinkTeslaAccountCommandHandler(
            UserRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UnlinksTeslaAccountSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("tesla123");

        UnlinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(command.UserId);
        result.TeslaAccount.Should().NotBeNull();
        result.TeslaAccount!.IsActive.Should().BeFalse();

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unlinked Tesla account")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        UnlinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid()
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoTeslaAccountLinked_ThrowsValidationException()
    {
        // Arrange
        User user = CreateTestUser();
        UnlinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("TeslaAccount");
        exception.Errors["TeslaAccount"].Should().Contain("No Tesla account linked");

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TeslaAccountAlreadyInactive_StillUnlinksSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount(); // Already unlinked

        // Reset domain events to track new ones
        user.ClearDomainEvents();

        UnlinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TeslaAccount.Should().NotBeNull();
        result.TeslaAccount!.IsActive.Should().BeFalse();

        // Should still call update and save even if already inactive
        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
