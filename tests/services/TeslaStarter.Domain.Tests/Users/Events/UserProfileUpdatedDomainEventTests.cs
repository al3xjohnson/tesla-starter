namespace TeslaStarter.Domain.Tests.Users.Events;

public class UserProfileUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        string oldEmail = "old@example.com";
        string newEmail = "new@example.com";
        string oldDisplayName = "Old Name";
        string newDisplayName = "New Name";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserProfileUpdatedDomainEvent @event = new UserProfileUpdatedDomainEvent(userId, oldEmail, newEmail, oldDisplayName, newDisplayName);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.OldEmail.Should().Be(oldEmail);
        @event.NewEmail.Should().Be(newEmail);
        @event.OldDisplayName.Should().Be(oldDisplayName);
        @event.NewDisplayName.Should().Be(newDisplayName);
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullValues_InitializesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();

        // Act
        UserProfileUpdatedDomainEvent @event = new UserProfileUpdatedDomainEvent(userId, null, "new@example.com", null, "Name");

        // Assert
        @event.OldEmail.Should().BeNull();
        @event.NewEmail.Should().Be("new@example.com");
        @event.OldDisplayName.Should().BeNull();
        @event.NewDisplayName.Should().Be("Name");
    }

    [Fact]
    public void Constructor_WithOnlyEmailChange_InitializesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();

        // Act
        UserProfileUpdatedDomainEvent @event = new UserProfileUpdatedDomainEvent(userId, "old@example.com", "new@example.com", null, null);

        // Assert
        @event.OldEmail.Should().Be("old@example.com");
        @event.NewEmail.Should().Be("new@example.com");
        @event.OldDisplayName.Should().BeNull();
        @event.NewDisplayName.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithOnlyDisplayNameChange_InitializesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        string email = "test@example.com";

        // Act
        UserProfileUpdatedDomainEvent @event = new UserProfileUpdatedDomainEvent(userId, null, email, "Old Name", "New Name");

        // Assert
        @event.OldEmail.Should().BeNull();
        @event.NewEmail.Should().Be(email);
        @event.OldDisplayName.Should().Be("Old Name");
        @event.NewDisplayName.Should().Be("New Name");
    }

    [Fact]
    public void Constructor_EachInstanceHasUniqueId()
    {
        // Arrange
        UserId userId = UserId.New();

        // Act
        UserProfileUpdatedDomainEvent event1 = new UserProfileUpdatedDomainEvent(userId, null, "test@example.com", null, null);
        UserProfileUpdatedDomainEvent event2 = new UserProfileUpdatedDomainEvent(userId, null, "test@example.com", null, null);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserProfileUpdatedDomainEvent @event = new UserProfileUpdatedDomainEvent(UserId.New(), null, "test@example.com", null, null);

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
