namespace TeslaStarter.Application.Common.Interfaces;

public interface ITeslaOAuthService
{
    string GenerateAuthorizationUrl(string state);
    Task<TeslaTokenResponse?> ExchangeCodeForTokensAsync(string code);
    Task<TeslaTokenResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token);
}

public class TeslaTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);
}

public class TeslaOAuthState
{
    public string State { get; set; } = string.Empty;
    public string DescopeUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExpired => DateTime.UtcNow > CreatedAt.AddMinutes(10);
}
