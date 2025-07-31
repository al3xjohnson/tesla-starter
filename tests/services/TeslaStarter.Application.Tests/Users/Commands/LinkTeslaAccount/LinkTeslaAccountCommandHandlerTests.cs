using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Commands.LinkTeslaAccount;

public sealed class LinkTeslaAccountCommandHandlerTests : ApplicationTestBase
{
    private readonly LinkTeslaAccountCommandHandler _handler;
    private readonly Mock<ILogger<LinkTeslaAccountCommandHandler>> _loggerMock;

    public LinkTeslaAccountCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<LinkTeslaAccountCommandHandler>();
        _handler = new LinkTeslaAccountCommandHandler(
            UserRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_LinksTeslaAccountSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        LinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value,
            TeslaAccountId = "tesla123"
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
        result.TeslaAccount!.TeslaAccountId.Should().Be(command.TeslaAccountId);

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Linked Tesla account")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        LinkTeslaAccountCommand command = new()
        {
            UserId = Guid.NewGuid(),
            TeslaAccountId = "tesla123"
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
    public async Task Handle_UserAlreadyHasActiveTeslaAccount_ThrowsValidationException()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("existing123");

        LinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value,
            TeslaAccountId = "tesla123"
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("TeslaAccountId");
        exception.Errors["TeslaAccountId"].Should().Contain("User already has an active Tesla account linked");

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserHasInactiveTeslaAccount_LinksNewAccountSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("existing123");
        user.UnlinkTeslaAccount(); // Make it inactive

        LinkTeslaAccountCommand command = new()
        {
            UserId = user.Id.Value,
            TeslaAccountId = "tesla123"
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
        result.TeslaAccount!.TeslaAccountId.Should().Be(command.TeslaAccountId);
        result.TeslaAccount.IsActive.Should().BeTrue();

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
