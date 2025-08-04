using System.Diagnostics.CodeAnalysis;
using Common.Domain.Persistence;
using FluentAssertions;
using Moq;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;
using TeslaStarter.Infrastructure.Authentication;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Authentication;

public sealed class UserSynchronizationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserSynchronizationService _service;

    public UserSynchronizationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UserSynchronizationService(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserDoesNotExist_CreatesNewUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string name = "Test User";
        ExternalId externalId = ExternalId.Create(descopeUserId);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, name);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.Is<ExternalId>(id => id.Value == descopeUserId),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(x => x.Add(
            It.Is<User>(u =>
                u.ExternalId.Value == descopeUserId &&
                u.Email!.Value == email &&
                u.DisplayName == name)),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserDoesNotExist_CreatesNewUserWithNullName()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string? name = null;

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, name);

        // Assert
        _userRepositoryMock.Verify(x => x.Add(
            It.Is<User>(u =>
                u.ExternalId.Value == descopeUserId &&
                u.Email!.Value == email &&
                u.DisplayName == null)),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsWithSameData_DoesNotUpdate()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string name = "Test User";

        User existingUser = User.Create(descopeUserId, email, name);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, name);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsWithDifferentEmail_UpdatesUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string oldEmail = "old@example.com";
        string newEmail = "new@example.com";
        string name = "Test User";

        User existingUser = User.Create(descopeUserId, oldEmail, name);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, newEmail, name);

        // Assert
        existingUser.Email!.Value.Should().Be(newEmail);
        _userRepositoryMock.Verify(x => x.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsWithDifferentName_UpdatesUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string oldName = "Old Name";
        string newName = "New Name";

        User existingUser = User.Create(descopeUserId, email, oldName);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, newName);

        // Assert
        existingUser.DisplayName.Should().Be(newName);
        _userRepositoryMock.Verify(x => x.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsWithDifferentEmailAndName_UpdatesUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string oldEmail = "old@example.com";
        string newEmail = "new@example.com";
        string oldName = "Old Name";
        string newName = "New Name";

        User existingUser = User.Create(descopeUserId, oldEmail, oldName);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, newEmail, newName);

        // Assert
        existingUser.Email!.Value.Should().Be(newEmail);
        existingUser.DisplayName.Should().Be(newName);
        _userRepositoryMock.Verify(x => x.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsAndNameChangesToNull_UpdatesUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string oldName = "Old Name";
        string? newName = null;

        User existingUser = User.Create(descopeUserId, email, oldName);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, newName);

        // Assert
        existingUser.DisplayName.Should().BeNull();
        _userRepositoryMock.Verify(x => x.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WhenUserExistsWithNullNameAndNewNameProvided_UpdatesUser()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string? oldName = null;
        string newName = "New Name";

        User existingUser = User.Create(descopeUserId, email, oldName);

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, newName);

        // Assert
        existingUser.DisplayName.Should().Be(newName);
        _userRepositoryMock.Verify(x => x.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SynchronizeUserAsync_WithCancellationToken_PassesTokenToAllMethods()
    {
        // Arrange
        string descopeUserId = "descope123";
        string email = "test@example.com";
        string name = "Test User";
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(It.IsAny<ExternalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _service.SynchronizeUserAsync(descopeUserId, email, name, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByExternalIdAsync(
            It.IsAny<ExternalId>(),
            cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
