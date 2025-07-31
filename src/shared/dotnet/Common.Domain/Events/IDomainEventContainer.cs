namespace Common.Domain.Events;

public interface IDomainEventContainer
{
    IEnumerable<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
