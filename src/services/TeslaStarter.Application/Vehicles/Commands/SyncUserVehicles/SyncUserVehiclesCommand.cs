namespace TeslaStarter.Application.Vehicles.Commands.SyncUserVehicles;

public record SyncUserVehiclesCommand : IRequest<int>
{
    public string ExternalId { get; init; } = string.Empty;
}
