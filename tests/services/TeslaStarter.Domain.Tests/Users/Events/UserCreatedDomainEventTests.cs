namespace TeslaStarter.Domain.Tests.Users.Events;

public class UserCreatedDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        string externalId = "auth0|123";
        string email = "test@example.com";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserCreatedDomainEvent @event = new UserCreatedDomainEvent(userId, externalId, email);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.ExternalId.Should().Be(externalId);
        @event.Email.Should().Be(email);
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_EachInstanceHasUniqueId()
    {
        // Arrange
        UserId userId = UserId.New();

        // Act
        UserCreatedDomainEvent event1 = new UserCreatedDomainEvent(userId, "auth0|123", "test@example.com");
        UserCreatedDomainEvent event2 = new UserCreatedDomainEvent(userId, "auth0|123", "test@example.com");

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void RecordEquality_WithSameData_PropertiesMatch()
    {
        // Arrange
        UserId userId = UserId.New();
        UserCreatedDomainEvent event1 = new UserCreatedDomainEvent(userId, "auth0|123", "test@example.com");
        UserCreatedDomainEvent event2 = new UserCreatedDomainEvent(userId, "auth0|123", "test@example.com");

        // Assert
        // Records are equal if all their properties are equal
        event1.UserId.Should().Be(event2.UserId);
        event1.ExternalId.Should().Be(event2.ExternalId);
        event1.Email.Should().Be(event2.Email);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ReturnsFalse()
    {
        // Arrange
        UserCreatedDomainEvent event1 = new UserCreatedDomainEvent(UserId.New(), "auth0|123", "test@example.com");
        UserCreatedDomainEvent event2 = new UserCreatedDomainEvent(UserId.New(), "auth0|456", "other@example.com");

        // Assert
        event1.Should().NotBe(event2);
        (event1 != event2).Should().BeTrue();
    }

    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserCreatedDomainEvent @event = new UserCreatedDomainEvent(UserId.New(), "auth0|123", "test@example.com");

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
