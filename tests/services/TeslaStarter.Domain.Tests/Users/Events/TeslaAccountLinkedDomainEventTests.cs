namespace TeslaStarter.Domain.Tests.Users.Events;

public class TeslaAccountLinkedDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "tesla123";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaAccountLinkedDomainEvent @event = new TeslaAccountLinkedDomainEvent(userId, teslaAccountId);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.TeslaAccountId.Should().Be(teslaAccountId);
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_EachInstanceHasUniqueId()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "tesla123";
        // Act
        TeslaAccountLinkedDomainEvent event1 = new TeslaAccountLinkedDomainEvent(userId, teslaAccountId);
        TeslaAccountLinkedDomainEvent event2 = new TeslaAccountLinkedDomainEvent(userId, teslaAccountId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void RecordEquality_WithSameData_PropertiesMatch()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "tesla123";
        TeslaAccountLinkedDomainEvent event1 = new TeslaAccountLinkedDomainEvent(userId, teslaAccountId);
        TeslaAccountLinkedDomainEvent event2 = new TeslaAccountLinkedDomainEvent(userId, teslaAccountId);

        // Assert
        event1.UserId.Should().Be(event2.UserId);
        event1.TeslaAccountId.Should().Be(event2.TeslaAccountId);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ReturnsFalse()
    {
        // Arrange
        TeslaAccountLinkedDomainEvent event1 = new TeslaAccountLinkedDomainEvent(UserId.New(), "tesla123");
        TeslaAccountLinkedDomainEvent event2 = new TeslaAccountLinkedDomainEvent(UserId.New(), "tesla456");

        // Assert
        event1.Should().NotBe(event2);
    }


    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaAccountLinkedDomainEvent @event = new TeslaAccountLinkedDomainEvent(UserId.New(), "tesla123");

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
