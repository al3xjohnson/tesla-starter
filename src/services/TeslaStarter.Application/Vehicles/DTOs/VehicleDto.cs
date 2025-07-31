namespace TeslaStarter.Application.Vehicles.DTOs;

public record VehicleDto
{
    public Guid Id { get; init; }
    public string TeslaAccountId { get; init; } = string.Empty;
    public string VehicleIdentifier { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public DateTime LinkedAt { get; init; }
    public DateTime? LastSyncedAt { get; init; }
    public bool IsActive { get; init; }
}
