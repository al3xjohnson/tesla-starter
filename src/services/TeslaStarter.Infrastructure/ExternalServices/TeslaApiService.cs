using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TeslaStarter.Application.Common.Interfaces;

namespace TeslaStarter.Infrastructure.ExternalServices;

public class TeslaApiService(
    HttpClient httpClient,
    ILogger<TeslaApiService> logger) : ITeslaApiService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<TeslaApiService> _logger = logger;
    private const string TeslaApiBaseUrl = "https://fleet-api.prd.vn.cloud.tesla.com";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<TeslaVehicleDto>> GetVehiclesAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync($"{TeslaApiBaseUrl}/api/1/vehicles");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch vehicles from Tesla API: {StatusCode}", response.StatusCode);
                return [];
            }

            string content = await response.Content.ReadAsStringAsync();
            TeslaVehiclesResponse? vehiclesResponse = JsonSerializer.Deserialize<TeslaVehiclesResponse>(content, JsonOptions);

            if (vehiclesResponse?.Response == null)
            {
                return [];
            }

            return [.. vehiclesResponse.Response.Select(v => new TeslaVehicleDto
            {
                Id = v.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Vin = v.Vin ?? string.Empty,
                DisplayName = v.DisplayName ?? string.Empty,
                State = v.State ?? string.Empty
            })];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicles from Tesla API");
            return [];
        }
    }
}

internal sealed class TeslaVehiclesResponse
{
    public List<TeslaVehicle>? Response { get; set; }
    public int? Count { get; set; }
}

internal sealed class TeslaVehicle
{
    public long Id { get; set; }
    public long? VehicleId { get; set; }
    public string? Vin { get; set; }
    public string? DisplayName { get; set; }
    public string? State { get; set; }
    public string? OptionCodes { get; set; }
    public string? Color { get; set; }
    public List<string>? Tokens { get; set; }
    public bool? InService { get; set; }
    public bool? CalendarEnabled { get; set; }
    public string? ApiVersion { get; set; }
}
