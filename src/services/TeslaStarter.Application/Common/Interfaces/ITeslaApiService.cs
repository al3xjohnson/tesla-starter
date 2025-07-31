namespace TeslaStarter.Application.Common.Interfaces;

public interface ITeslaApiService
{
    Task<IReadOnlyList<TeslaVehicleDto>> GetVehiclesAsync(string accessToken);
}

public record TeslaVehicleDto
{
    public string Id { get; init; } = string.Empty;
    public string Vin { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}
