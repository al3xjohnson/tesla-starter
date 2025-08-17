namespace TeslaStarter.Domain.Users;

public sealed record UserLoggedInDomainEvent(
    UserId UserId,
    DateTime LoginTime) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();

}
