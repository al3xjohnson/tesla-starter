namespace Common.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid Id { get; }
}
