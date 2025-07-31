using System.Diagnostics.CodeAnalysis;
using Common.Domain.Persistence;
using FluentAssertions;
using Moq;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Application.Users.Commands.UpdateUserTeslaTokens;
using TeslaStarter.Domain.Users;
using Xunit;

namespace TeslaStarter.Application.Tests.Users.Commands.UpdateUserTeslaTokens;

[ExcludeFromCodeCoverage(Justification = "Test class")]
public sealed class UpdateUserTeslaTokensCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserTeslaTokensCommandHandler _handler;

    public UpdateUserTeslaTokensCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateUserTeslaTokensCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = "nonexistent",
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            TeslaAccountId = "tesla123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with External ID nonexistent not found");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoTeslaAccount_LinksAccountAndUpdatesTokens()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");

        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = externalId,
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            TeslaAccountId = teslaAccountId
        };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.TeslaAccountId.Value.Should().Be(teslaAccountId);
        user.TeslaAccount.IsActive.Should().BeTrue();
        user.TeslaAccount.AccessToken.Should().Be(command.AccessToken);
        user.TeslaAccount.RefreshToken.Should().Be(command.RefreshToken);
        user.TeslaAccount.TokenExpiresAt.Should().Be(command.ExpiresAt);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasInactiveTeslaAccount_ReactivatesAndUpdatesTokens()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);
        user.UnlinkTeslaAccount(); // Make it inactive

        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = externalId,
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            TeslaAccountId = teslaAccountId
        };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.IsActive.Should().BeTrue();
        user.TeslaAccount.AccessToken.Should().Be(command.AccessToken);
        user.TeslaAccount.RefreshToken.Should().Be(command.RefreshToken);
        user.TeslaAccount.TokenExpiresAt.Should().Be(command.ExpiresAt);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasActiveTeslaAccount_OnlyUpdatesTokens()
    {
        // Arrange
        string externalId = "descope123";
        string teslaAccountId = "tesla123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        user.LinkTeslaAccount(teslaAccountId);

        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = externalId,
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            TeslaAccountId = teslaAccountId
        };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.TeslaAccountId.Value.Should().Be(teslaAccountId);
        user.TeslaAccount.IsActive.Should().BeTrue();
        user.TeslaAccount.AccessToken.Should().Be(command.AccessToken);
        user.TeslaAccount.RefreshToken.Should().Be(command.RefreshToken);
        user.TeslaAccount.TokenExpiresAt.Should().Be(command.ExpiresAt);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToAllMethods()
    {
        // Arrange
        string externalId = "descope123";
        User user = User.Create(externalId, "test@example.com", "Test User");
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = externalId,
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            TeslaAccountId = "tesla123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
