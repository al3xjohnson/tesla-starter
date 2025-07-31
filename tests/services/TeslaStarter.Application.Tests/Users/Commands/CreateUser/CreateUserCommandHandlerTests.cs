using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateUserCommandHandler _handler;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;

    public CreateUserCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<CreateUserCommandHandler>();
        _handler = new CreateUserCommandHandler(
            UserRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserSuccessfully()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(command.ExternalId);
        result.Email.Should().Be(command.Email);
        result.DisplayName.Should().Be(command.DisplayName);

        UserRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingExternalId_ThrowsValidationException()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        User existingUser = CreateTestUser();
        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("ExternalId");
        exception.Errors["ExternalId"].Should().Contain("A user with this external ID already exists.");

        UserRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsValidationException()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User existingUser = CreateTestUser();
        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("Email");
        exception.Errors["Email"].Should().Contain("A user with this email already exists.");

        UserRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutDisplayName_CreatesUserSuccessfully()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = null
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().BeNull();

        UserRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
