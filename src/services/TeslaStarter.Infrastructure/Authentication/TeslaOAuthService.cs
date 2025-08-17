using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Infrastructure.Configuration;

namespace TeslaStarter.Infrastructure.Authentication;

public class TeslaOAuthService(
    HttpClient httpClient,
    IOptions<TeslaOptions> teslaOptions,
    ILogger<TeslaOAuthService> logger) : ITeslaOAuthService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TeslaOptions _teslaOptions = teslaOptions.Value;
    private readonly ILogger<TeslaOAuthService> _logger = logger;

    private const string TeslaAuthBaseUrl = "https://fleet-auth.prd.vn.cloud.tesla.com";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string GenerateAuthorizationUrl(string state)
    {
        StringBuilder authUrl = new($"{TeslaAuthBaseUrl}/oauth2/v3/authorize");
        authUrl.Append(System.Globalization.CultureInfo.InvariantCulture, $"?client_id={Uri.EscapeDataString(_teslaOptions.ClientId)}");
        authUrl.Append(System.Globalization.CultureInfo.InvariantCulture, $"&redirect_uri={Uri.EscapeDataString(_teslaOptions.RedirectUri?.ToString() ?? "")}");
        authUrl.Append("&response_type=code");
        authUrl.Append("&scope=openid%20vehicle_device_data%20offline_access");
        authUrl.Append(System.Globalization.CultureInfo.InvariantCulture, $"&state={Uri.EscapeDataString(state)}");

        return authUrl.ToString();
    }

    public async Task<TeslaTokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        try
        {
            Dictionary<string, string> requestBody = new()
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code!,
                ["redirect_uri"] = _teslaOptions.RedirectUri.ToString(),
                ["client_id"] = _teslaOptions.ClientId,
                ["client_secret"] = _teslaOptions.ClientSecret
            };

            HttpRequestMessage request = new(HttpMethod.Post, $"{TeslaAuthBaseUrl}/oauth2/v3/token")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to exchange Tesla code for tokens: {StatusCode} - {Error}",
                    response.StatusCode, error);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            TeslaTokenDto? dto = JsonSerializer.Deserialize<TeslaTokenDto>(json, JsonOptions);

            if (dto == null)
                return null;

            return new TeslaTokenResponse
            {
                AccessToken = dto.AccessToken ?? "",
                RefreshToken = dto.RefreshToken ?? "",
                IdToken = dto.IdToken ?? "",
                TokenType = dto.TokenType ?? "Bearer",
                ExpiresIn = dto.ExpiresIn ?? 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Tesla code for tokens");
            return null;
        }
    }

    public async Task<TeslaTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            Dictionary<string, string> requestBody = new()
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _teslaOptions.ClientId,
                ["client_secret"] = _teslaOptions.ClientSecret
            };

            HttpRequestMessage request = new(HttpMethod.Post, $"{TeslaAuthBaseUrl}/oauth2/v3/token")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refresh Tesla token: {StatusCode} - {Error}",
                    response.StatusCode, error);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            TeslaTokenDto? dto = JsonSerializer.Deserialize<TeslaTokenDto>(json, JsonOptions);

            if (dto == null)
                return null;

            return new TeslaTokenResponse
            {
                AccessToken = dto.AccessToken ?? "",
                RefreshToken = dto.RefreshToken ?? "",
                IdToken = dto.IdToken ?? "",
                TokenType = dto.TokenType ?? "Bearer",
                ExpiresIn = dto.ExpiresIn ?? 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Tesla token");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            Dictionary<string, string> requestBody = new()
            {
                ["token"] = token,
                ["client_id"] = _teslaOptions.ClientId
            };

            HttpRequestMessage request = new(HttpMethod.Post, $"{TeslaAuthBaseUrl}/oauth2/v3/revoke")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking Tesla token");
            return false;
        }
    }


    public static string GenerateState()
    {
        byte[] bytes = new byte[16];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string GetUserIdFromToken(string idToken)
    {
        try
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jsonToken = handler.ReadJwtToken(idToken);

            return jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
