using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Queries.GetUserByExternalId;

public sealed class GetUserByExternalIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserByExternalIdQueryHandler _handler;

    public GetUserByExternalIdQueryHandlerTests()
    {
        _handler = new GetUserByExternalIdQueryHandler(
            UserRepositoryMock.Object,
            Mapper);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUserDto()
    {
        // Arrange
        User user = CreateTestUser("ext123", "test@example.com", "Test User");
        GetUserByExternalIdQuery query = new()
        {
            ExternalId = "ext123"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id.Value);
        result.ExternalId.Should().Be("ext123");
        result.Email.Should().Be(user.Email.Value);
        result.DisplayName.Should().Be(user.DisplayName);

        UserRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.Is<ExternalId>(e => e.Value == "ext123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        GetUserByExternalIdQuery query = new()
        {
            ExternalId = "nonexistent"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        UserRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithTeslaAccount_ReturnsDtoWithTeslaAccount()
    {
        // Arrange
        User user = CreateTestUser("ext123");
        user.LinkTeslaAccount("tesla123");

        GetUserByExternalIdQuery query = new()
        {
            ExternalId = "ext123"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TeslaAccount.Should().NotBeNull();
        result.TeslaAccount!.TeslaAccountId.Should().Be("tesla123");
    }

    [Fact]
    public async Task Handle_UserWithoutDisplayName_ReturnsDtoWithNullDisplayName()
    {
        // Arrange
        User user = CreateTestUser("ext123", "test@example.com", null);

        GetUserByExternalIdQuery query = new()
        {
            ExternalId = "ext123"
        };

        UserRepositoryMock.Setup(x => x.GetByExternalIdAsync(
                It.IsAny<ExternalId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyExternalId_ThrowsArgumentException()
    {
        // Arrange
        GetUserByExternalIdQuery query = new()
        {
            ExternalId = ""
        };

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Contain("External ID cannot be empty");

        UserRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
