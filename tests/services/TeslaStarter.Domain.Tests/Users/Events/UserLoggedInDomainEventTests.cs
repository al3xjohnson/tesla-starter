namespace TeslaStarter.Domain.Tests.Users.Events;

public class UserLoggedInDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        DateTime loginTime = DateTime.UtcNow.AddMinutes(-5);
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserLoggedInDomainEvent @event = new UserLoggedInDomainEvent(userId, loginTime);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.LoginTime.Should().Be(loginTime);
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_EachInstanceHasUniqueId()
    {
        // Arrange
        UserId userId = UserId.New();
        DateTime loginTime = DateTime.UtcNow;

        // Act
        UserLoggedInDomainEvent event1 = new UserLoggedInDomainEvent(userId, loginTime);
        UserLoggedInDomainEvent event2 = new UserLoggedInDomainEvent(userId, loginTime);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void LoginTime_CanBeDifferentFromOccurredOn()
    {
        // Arrange
        UserId userId = UserId.New();
        DateTime loginTime = DateTime.UtcNow.AddHours(-1);

        // Act
        UserLoggedInDomainEvent @event = new UserLoggedInDomainEvent(userId, loginTime);

        // Assert
        @event.LoginTime.Should().Be(loginTime);
        @event.OccurredOn.Should().BeAfter(loginTime);
    }

    [Fact]
    public void RecordEquality_WithSameData_PropertiesMatch()
    {
        // Arrange
        UserId userId = UserId.New();
        DateTime loginTime = DateTime.UtcNow;
        UserLoggedInDomainEvent event1 = new UserLoggedInDomainEvent(userId, loginTime);
        UserLoggedInDomainEvent event2 = new UserLoggedInDomainEvent(userId, loginTime);

        // Assert
        event1.UserId.Should().Be(event2.UserId);
        event1.LoginTime.Should().Be(event2.LoginTime);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ReturnsFalse()
    {
        // Arrange
        UserLoggedInDomainEvent event1 = new UserLoggedInDomainEvent(UserId.New(), DateTime.UtcNow);
        UserLoggedInDomainEvent event2 = new UserLoggedInDomainEvent(UserId.New(), DateTime.UtcNow.AddMinutes(1));

        // Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        UserLoggedInDomainEvent @event = new UserLoggedInDomainEvent(UserId.New(), DateTime.UtcNow.AddMinutes(-30));

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
