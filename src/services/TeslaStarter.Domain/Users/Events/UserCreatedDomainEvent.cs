namespace TeslaStarter.Domain.Users;

public sealed record UserCreatedDomainEvent(
    UserId UserId,
    string ExternalId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();

}
