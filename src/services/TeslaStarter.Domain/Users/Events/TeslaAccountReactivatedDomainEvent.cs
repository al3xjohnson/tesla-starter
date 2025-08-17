namespace TeslaStarter.Domain.Users;

public sealed record TeslaAccountReactivatedDomainEvent(
    UserId UserId,
    string TeslaAccountId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();
}
