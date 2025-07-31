using TeslaStarter.Application.Tests.TestHelpers;

namespace TeslaStarter.Application.Tests.Users.Queries.GetUser;

public sealed class GetUserQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _handler = new GetUserQueryHandler(
            UserRepositoryMock.Object,
            Mapper);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUserDto()
    {
        // Arrange
        User user = CreateTestUser();
        GetUserQuery query = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id.Value);
        result.ExternalId.Should().Be(user.ExternalId.Value);
        result.Email.Should().Be(user.Email.Value);
        result.DisplayName.Should().Be(user.DisplayName);

        UserRepositoryMock.Verify(x => x.GetByIdAsync(
            It.IsAny<UserId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        GetUserQuery query = new()
        {
            UserId = Guid.NewGuid()
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(
            It.IsAny<UserId>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithTeslaAccount_ReturnsDtoWithTeslaAccount()
    {
        // Arrange
        User user = CreateTestUser();
        user.LinkTeslaAccount("tesla123");

        GetUserQuery query = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TeslaAccount.Should().NotBeNull();
        result.TeslaAccount!.TeslaAccountId.Should().Be("tesla123");
        result.TeslaAccount.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserWithoutTeslaAccount_ReturnsDtoWithNullTeslaAccount()
    {
        // Arrange
        User user = CreateTestUser();

        GetUserQuery query = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TeslaAccount.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserWithLastLogin_ReturnsDtoWithLastLoginAt()
    {
        // Arrange
        User user = CreateTestUser();
        user.RecordLogin();

        GetUserQuery query = new()
        {
            UserId = user.Id.Value
        };

        UserRepositoryMock.Setup(x => x.GetByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().Be(user.LastLoginAt);
    }
}
