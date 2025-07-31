namespace TeslaStarter.Domain.Tests.Users.Events;

public class TeslaAccountUnlinkedDomainEventTests
{
    [Fact]
    public void Constructor_InitializesAllPropertiesCorrectly()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "tesla123";
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaAccountUnlinkedDomainEvent @event = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);

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
        TeslaAccountUnlinkedDomainEvent event1 = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);
        TeslaAccountUnlinkedDomainEvent event2 = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void RecordEquality_WithSameData_PropertiesMatch()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "tesla123";
        TeslaAccountUnlinkedDomainEvent event1 = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);
        TeslaAccountUnlinkedDomainEvent event2 = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);

        // Assert
        event1.UserId.Should().Be(event2.UserId);
        event1.TeslaAccountId.Should().Be(event2.TeslaAccountId);
    }

    [Fact]
    public void RecordEquality_WithDifferentData_ReturnsFalse()
    {
        // Arrange
        TeslaAccountUnlinkedDomainEvent event1 = new TeslaAccountUnlinkedDomainEvent(UserId.New(), "tesla123");
        TeslaAccountUnlinkedDomainEvent event2 = new TeslaAccountUnlinkedDomainEvent(UserId.New(), "tesla456");

        // Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void TeslaAccountId_PreservesOriginalValue()
    {
        // Arrange
        UserId userId = UserId.New();
        string teslaAccountId = "original_tesla_123";

        // Act
        TeslaAccountUnlinkedDomainEvent @event = new TeslaAccountUnlinkedDomainEvent(userId, teslaAccountId);

        // Assert
        @event.TeslaAccountId.Should().Be(teslaAccountId);
    }

    [Fact]
    public void OccurredOn_IsSetToCurrentUtcTime()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        TeslaAccountUnlinkedDomainEvent @event = new TeslaAccountUnlinkedDomainEvent(UserId.New(), "tesla123");

        // Assert
        @event.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }
}
