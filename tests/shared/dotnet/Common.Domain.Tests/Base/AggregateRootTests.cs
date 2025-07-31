using Common.Domain.Base;
using Common.Domain.Events;

namespace Common.Domain.Tests.Base;

// Test domain event for testing
internal sealed record TestDomainEvent(string Message) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
}

// Test aggregate root for testing the AggregateRoot base class
internal sealed class TestAggregateRoot : AggregateRoot<Guid>
{
    public TestAggregateRoot(Guid id) : base(id) { }

    // Constructor for EF Core - this will cover the parameterless constructor
    public TestAggregateRoot() : base() { }

    // Public method to expose RaiseDomainEvent for testing
    public void RaiseTestEvent(IDomainEvent domainEvent)
    {
        RaiseDomainEvent(domainEvent);
    }
}

public class AggregateRootTests
{
    [Fact]
    public void Constructor_WithId_ShouldSetIdAndInitializeEmptyDomainEvents()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        TestAggregateRoot aggregate = new(id);

        // Assert
        aggregate.Id.Should().Be(id);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ParameterlessConstructor_ShouldSetDefaultIdAndInitializeEmptyDomainEvents()
    {
        // Act
        TestAggregateRoot aggregate = new();

        // Assert
        aggregate.Id.Should().Be(default(Guid));
        aggregate.DomainEvents.Should().BeEmpty();
    }


    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        TestDomainEvent domainEvent = new("Test message");

        // Act
        aggregate.RaiseTestEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void RaiseDomainEvent_WithMultipleEvents_ShouldAddAllEvents()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        TestDomainEvent event1 = new("First event");
        TestDomainEvent event2 = new("Second event");

        // Act
        aggregate.RaiseTestEvent(event1);
        aggregate.RaiseTestEvent(event2);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(2);
        aggregate.DomainEvents.Should().Contain(event1);
        aggregate.DomainEvents.Should().Contain(event2);
        aggregate.DomainEvents[0].Should().Be(event1);
        aggregate.DomainEvents[1].Should().Be(event2);
    }

    [Fact]
    public void ClearDomainEvents_WithNoEvents_ShouldRemainEmpty()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WithExistingEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        TestDomainEvent event1 = new("First event");
        TestDomainEvent event2 = new("Second event");

        aggregate.RaiseTestEvent(event1);
        aggregate.RaiseTestEvent(event2);

        // Verify events were added
        aggregate.DomainEvents.Should().HaveCount(2);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }


    [Fact]
    public void DomainEvents_ShouldPreserveEventOrder()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        List<TestDomainEvent> events = [.. Enumerable.Range(1, 5).Select(i => new TestDomainEvent($"Event {i}"))];

        // Act
        foreach (TestDomainEvent evt in events)
        {
            aggregate.RaiseTestEvent(evt);
        }

        // Assert
        aggregate.DomainEvents.Should().HaveCount(5);
        for (int i = 0; i < events.Count; i++)
        {
            aggregate.DomainEvents[i].Should().Be(events[i]);
        }
    }

    [Fact]
    public void RaiseDomainEvent_WithSameEventMultipleTimes_ShouldAddEachInstance()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        TestDomainEvent domainEvent = new("Repeated event");

        // Act
        aggregate.RaiseTestEvent(domainEvent);
        aggregate.RaiseTestEvent(domainEvent);
        aggregate.RaiseTestEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().AllBeEquivalentTo(domainEvent);
    }

    [Fact]
    public void AggregateRoot_ShouldInheritFromEntity()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());

        // Assert
        aggregate.Should().BeAssignableTo<Entity<Guid>>();
    }

    [Fact]
    public void GetDomainEvents_ShouldReturnCopyOfDomainEvents()
    {
        // Arrange
        TestAggregateRoot aggregate = new(Guid.NewGuid());
        TestDomainEvent event1 = new("First event");
        TestDomainEvent event2 = new("Second event");

        aggregate.RaiseTestEvent(event1);
        aggregate.RaiseTestEvent(event2);

        // Act
        IEnumerable<IDomainEvent> domainEvents = aggregate.GetDomainEvents();

        // Assert
        domainEvents.Should().NotBeNull();
        domainEvents.Should().HaveCount(2);
        domainEvents.Should().Contain(event1);
        domainEvents.Should().Contain(event2);
        domainEvents.Should().BeEquivalentTo(aggregate.DomainEvents);
    }

}
