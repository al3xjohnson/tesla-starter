using Common.Domain.Events;
using Common.Domain.Persistence;

namespace TeslaStarter.Infrastructure.Persistence;

public class UnitOfWork(TeslaStarterDbContext context, IDomainEventDispatcher? domainEventDispatcher = null) : IUnitOfWork
{
    private readonly TeslaStarterDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IDomainEventDispatcher? _domainEventDispatcher = domainEventDispatcher;
    private bool _disposed;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(true, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await _context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }


    public void Clear()
    {
        _context.ChangeTracker.Clear();
    }

    public bool HasChanges()
    {
        _context.ChangeTracker.DetectChanges();
        return _context.ChangeTracker.HasChanges();
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_domainEventDispatcher == null)
            return;

        List<IDomainEvent> domainEvents = [.. _context.ChangeTracker
            .Entries()
            .Where(x => x.Entity is IDomainEventContainer)
            .Select(x => x.Entity)
            .Cast<IDomainEventContainer>()
            .SelectMany(container =>
            {
                List<IDomainEvent> domainEvents = [.. container.GetDomainEvents()];
                container.ClearDomainEvents();
                return domainEvents;
            })];

        await _domainEventDispatcher.DispatchEventsAsync(domainEvents, cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
