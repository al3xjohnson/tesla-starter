using Common.Domain.Events;

namespace TeslaStarter.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_CreatesUser()
    {
        // Arrange
        string externalId = "auth0|123";
        string email = "test@example.com";
        string displayName = "Test User";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        User user = User.Create(externalId, email, displayName);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(UserId.Empty);
        user.ExternalId.Value.Should().Be(externalId.Trim());
        user.Email.Value.Should().Be(email.Trim().ToLowerInvariant());
        user.DisplayName.Should().Be(displayName);
        user.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastLoginAt.Should().BeNull();
        user.TeslaAccount.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutDisplayName_CreatesUserWithNullDisplayName()
    {
        // Act
        User user = User.Create("auth0|123", "test@example.com");

        // Assert
        user.DisplayName.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesUserCreatedDomainEvent()
    {
        // Act
        User user = User.Create("auth0|123", "test@example.com", "Test User");

        // Assert
        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        UserCreatedDomainEvent createdEvent = domainEvents[0].Should().BeOfType<UserCreatedDomainEvent>().Subject;
        createdEvent.UserId.Should().Be(user.Id);
        createdEvent.ExternalId.Should().Be(user.ExternalId.Value);
        createdEvent.Email.Should().Be(user.Email.Value);
    }

    [Fact]
    public void UpdateProfile_WithNewEmail_UpdatesEmailAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "old@example.com", "Old Name");
        user.ClearDomainEvents(); // Clear creation event

        // Act
        user.UpdateProfile("new@example.com", "Old Name");

        // Assert
        user.Email.Value.Should().Be("new@example.com");
        user.DisplayName.Should().Be("Old Name");

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        UserProfileUpdatedDomainEvent updatedEvent = domainEvents[0].Should().BeOfType<UserProfileUpdatedDomainEvent>().Subject;
        updatedEvent.UserId.Should().Be(user.Id);
        updatedEvent.OldEmail.Should().Be("old@example.com");
        updatedEvent.NewEmail.Should().Be("new@example.com");
        updatedEvent.OldDisplayName.Should().BeNull();
        updatedEvent.NewDisplayName.Should().Be("Old Name");
    }

    [Fact]
    public void UpdateProfile_WithNewDisplayName_UpdatesDisplayNameAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com", "Old Name");
        user.ClearDomainEvents();

        // Act
        user.UpdateProfile("test@example.com", "New Name");

        // Assert
        user.Email.Value.Should().Be("test@example.com");
        user.DisplayName.Should().Be("New Name");

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        UserProfileUpdatedDomainEvent updatedEvent = domainEvents[0].Should().BeOfType<UserProfileUpdatedDomainEvent>().Subject;
        updatedEvent.OldEmail.Should().BeNull();
        updatedEvent.NewEmail.Should().Be("test@example.com");
        updatedEvent.OldDisplayName.Should().Be("Old Name");
        updatedEvent.NewDisplayName.Should().Be("New Name");
    }

    [Fact]
    public void UpdateProfile_WithBothChanges_UpdatesBothAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "old@example.com", "Old Name");
        user.ClearDomainEvents();

        // Act
        user.UpdateProfile("new@example.com", "New Name");

        // Assert
        user.Email.Value.Should().Be("new@example.com");
        user.DisplayName.Should().Be("New Name");

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        UserProfileUpdatedDomainEvent updatedEvent = domainEvents[0].Should().BeOfType<UserProfileUpdatedDomainEvent>().Subject;
        updatedEvent.OldEmail.Should().Be("old@example.com");
        updatedEvent.NewEmail.Should().Be("new@example.com");
        updatedEvent.OldDisplayName.Should().Be("Old Name");
        updatedEvent.NewDisplayName.Should().Be("New Name");
    }

    [Fact]
    public void UpdateProfile_WithNoChanges_DoesNotRaiseEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com", "Name");
        user.ClearDomainEvents();

        // Act
        user.UpdateProfile("test@example.com", "Name");

        // Assert
        user.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_FromNullToNullDisplayName_DoesNotRaiseEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.ClearDomainEvents();

        // Act
        user.UpdateProfile("test@example.com", null);

        // Assert
        // No event should be raised because nothing changed
        user.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void RecordLogin_UpdatesLastLoginAtAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.ClearDomainEvents();
        DateTime beforeLogin = DateTime.UtcNow;

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        UserLoggedInDomainEvent loginEvent = domainEvents[0].Should().BeOfType<UserLoggedInDomainEvent>().Subject;
        loginEvent.UserId.Should().Be(user.Id);
        loginEvent.LoginTime.Should().Be(user.LastLoginAt!.Value);
    }

    [Fact]
    public void LinkTeslaAccount_WithNoExistingAccount_LinksAccountAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.ClearDomainEvents();
        string teslaAccountId = "tesla123";

        // Act
        user.LinkTeslaAccount(teslaAccountId);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.TeslaAccountId.Value.Should().Be(teslaAccountId);
        user.TeslaAccount.IsActive.Should().BeTrue();

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        TeslaAccountLinkedDomainEvent linkedEvent = domainEvents[0].Should().BeOfType<TeslaAccountLinkedDomainEvent>().Subject;
        linkedEvent.UserId.Should().Be(user.Id);
        linkedEvent.TeslaAccountId.Should().Be(teslaAccountId);
    }

    [Fact]
    public void LinkTeslaAccount_WithActiveAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");

        // Act & Assert
        Action act = () => user.LinkTeslaAccount("tesla456");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User already has an active Tesla account linked");
    }

    [Fact]
    public void LinkTeslaAccount_WithInactiveAccount_LinksNewAccount()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount();
        user.ClearDomainEvents();

        // Act
        user.LinkTeslaAccount("tesla456");

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.TeslaAccountId.Value.Should().Be("tesla456");
        user.TeslaAccount.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UnlinkTeslaAccount_WithLinkedAccount_DeactivatesAccountAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        user.ClearDomainEvents();

        // Act
        user.UnlinkTeslaAccount();

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.IsActive.Should().BeFalse();
        user.TeslaAccount.RefreshToken.Should().BeNull();

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        TeslaAccountUnlinkedDomainEvent unlinkedEvent = domainEvents[0].Should().BeOfType<TeslaAccountUnlinkedDomainEvent>().Subject;
        unlinkedEvent.UserId.Should().Be(user.Id);
        unlinkedEvent.TeslaAccountId.Should().Be("tesla123");
    }

    [Fact]
    public void UnlinkTeslaAccount_WithNoAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");

        // Act & Assert
        Action act = () => user.UnlinkTeslaAccount();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No Tesla account linked");
    }

    [Fact]
    public void UpdateTeslaRefreshToken_WithActiveAccount_UpdatesToken()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        string newToken = "new_refresh_token";

        // Act
        user.UpdateTeslaRefreshToken(newToken);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.RefreshToken.Should().Be(newToken);
        user.TeslaAccount.LastSyncedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTeslaRefreshToken_WithNoAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");

        // Act & Assert
        Action act = () => user.UpdateTeslaRefreshToken("token");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No Tesla account linked");
    }

    [Fact]
    public void UpdateTeslaRefreshToken_WithInactiveAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount();

        // Act & Assert
        Action act = () => user.UpdateTeslaRefreshToken("token");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tesla account is not active");
    }

    [Fact]
    public void ReactivateTeslaAccount_WithInactiveAccount_ReactivatesAndRaisesEvent()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount();
        user.ClearDomainEvents();

        // Act
        user.ReactivateTeslaAccount();

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.IsActive.Should().BeTrue();

        List<IDomainEvent> domainEvents = user.GetDomainEvents().ToList();
        domainEvents.Should().HaveCount(1);

        TeslaAccountReactivatedDomainEvent reactivatedEvent = domainEvents[0].Should().BeOfType<TeslaAccountReactivatedDomainEvent>().Subject;
        reactivatedEvent.UserId.Should().Be(user.Id);
        reactivatedEvent.TeslaAccountId.Should().Be("tesla123");
    }

    [Fact]
    public void ReactivateTeslaAccount_WithNoAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");

        // Act & Assert
        Action act = () => user.ReactivateTeslaAccount();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No Tesla account linked");
    }

    [Fact]
    public void ReactivateTeslaAccount_WithActiveAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");

        // Act & Assert
        Action act = () => user.ReactivateTeslaAccount();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tesla account is already active");
    }

    [Fact]
    public void UpdateTeslaTokens_WithActiveAccount_UpdatesAllTokens()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        string newAccessToken = "new_access_token";
        string newRefreshToken = "new_refresh_token";
        DateTime expiresAt = DateTime.UtcNow.AddHours(8);

        // Act
        user.UpdateTeslaTokens(newAccessToken, newRefreshToken, expiresAt);

        // Assert
        user.TeslaAccount.Should().NotBeNull();
        user.TeslaAccount!.AccessToken.Should().Be(newAccessToken);
        user.TeslaAccount.RefreshToken.Should().Be(newRefreshToken);
        user.TeslaAccount.TokenExpiresAt.Should().Be(expiresAt);
        user.TeslaAccount.LastSyncedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTeslaTokens_WithNoAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");

        // Act & Assert
        Action act = () => user.UpdateTeslaTokens("access", "refresh", DateTime.UtcNow.AddHours(8));
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No Tesla account linked");
    }

    [Fact]
    public void UpdateTeslaTokens_WithInactiveAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        User user = User.Create("auth0|123", "test@example.com");
        user.LinkTeslaAccount("tesla123");
        user.UnlinkTeslaAccount();

        // Act & Assert
        Action act = () => user.UpdateTeslaTokens("access", "refresh", DateTime.UtcNow.AddHours(8));
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tesla account is not active");
    }

}
