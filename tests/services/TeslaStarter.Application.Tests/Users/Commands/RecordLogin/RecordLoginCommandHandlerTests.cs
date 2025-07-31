using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Commands.RecordLogin;

public sealed class RecordLoginCommandHandlerTests : ApplicationTestBase
{
    private readonly RecordLoginCommandHandler _handler;
    private readonly Mock<ILogger<RecordLoginCommandHandler>> _loggerMock;

    public RecordLoginCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<RecordLoginCommandHandler>();
        _handler = new RecordLoginCommandHandler(
            UserRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_RecordsLoginSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        DateTime? originalLastLogin = user.LastLoginAt;

        RecordLoginCommand command = new()
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
        result.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeAfter(originalLastLogin ?? DateTime.MinValue);

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recorded login")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        RecordLoginCommand command = new()
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
    public async Task Handle_UserWithExistingLastLogin_UpdatesLastLoginSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        user.RecordLogin(); // Set initial login
        DateTime? originalLastLogin = user.LastLoginAt;

        // Small delay to ensure time difference
        await Task.Delay(10);

        RecordLoginCommand command = new()
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
        result.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeAfter(originalLastLogin!.Value);

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
