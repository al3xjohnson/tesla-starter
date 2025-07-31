using System.ComponentModel.DataAnnotations.Schema;
using Common.Domain.Events;

namespace Common.Domain.Base;

public abstract class AggregateRoot<TId> : Entity<TId>, IDomainEventContainer where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() : base() { }

    [NotMapped]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public IEnumerable<IDomainEvent> GetDomainEvents()
    {
        return [.. _domainEvents];
    }
}
