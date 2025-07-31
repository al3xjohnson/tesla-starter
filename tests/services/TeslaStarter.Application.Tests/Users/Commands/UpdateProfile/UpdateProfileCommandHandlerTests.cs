using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateProfileCommandHandler _handler;
    private readonly Mock<ILogger<UpdateProfileCommandHandler>> _loggerMock;

    public UpdateProfileCommandHandlerTests()
    {
        _loggerMock = CreateLoggerMock<UpdateProfileCommandHandler>();
        _handler = new UpdateProfileCommandHandler(
            UserRepositoryMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesUserSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        UpdateProfileCommand command = new()
        {
            UserId = user.Id.Value,
            Email = "newemail@example.com",
            DisplayName = "New Display Name"
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(command.UserId);
        result.Email.Should().Be(command.Email);
        result.DisplayName.Should().Be(command.DisplayName);

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated profile")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        UpdateProfileCommand command = new()
        {
            UserId = Guid.NewGuid(),
            Email = "newemail@example.com",
            DisplayName = "New Display Name"
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
    public async Task Handle_EmailAlreadyTakenByAnotherUser_ThrowsValidationException()
    {
        // Arrange
        User user = CreateTestUser();
        User anotherUser = CreateTestUser("ext456", "another@example.com");

        UpdateProfileCommand command = new()
        {
            UserId = user.Id.Value,
            Email = "another@example.com",
            DisplayName = "New Display Name"
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(anotherUser);

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainKey("Email");
        exception.Errors["Email"].Should().Contain("Another user already has this email address.");

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameEmailAsCurrentUser_UpdatesSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        UpdateProfileCommand command = new()
        {
            UserId = user.Id.Value,
            Email = user.Email.Value, // Same email
            DisplayName = "New Display Name"
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user); // Same user

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(command.DisplayName);

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullDisplayName_UpdatesSuccessfully()
    {
        // Arrange
        User user = CreateTestUser();
        UpdateProfileCommand command = new()
        {
            UserId = user.Id.Value,
            Email = "newemail@example.com",
            DisplayName = null
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserRepositoryMock.Setup(x => x.GetByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        UserDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().BeNull();

        UserRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
