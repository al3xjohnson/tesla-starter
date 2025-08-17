namespace TeslaStarter.Domain.Users;

public sealed record UserProfileUpdatedDomainEvent(
    UserId UserId,
    string? OldEmail,
    string NewEmail,
    string? OldDisplayName,
    string? NewDisplayName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();

}
