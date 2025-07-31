namespace TeslaStarter.Domain.Vehicles.Events;

public sealed record VehicleLinkedDomainEvent(
    VehicleId VehicleId,
    TeslaAccountId TeslaAccountId,
    string VehicleIdentifier,
    string? DisplayName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid Id { get; } = Guid.NewGuid();
}
