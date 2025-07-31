namespace TeslaStarter.Domain.Users.Events;

public sealed record TeslaAccountLinkedDomainEvent(
    UserId UserId,
    string TeslaAccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();
}
