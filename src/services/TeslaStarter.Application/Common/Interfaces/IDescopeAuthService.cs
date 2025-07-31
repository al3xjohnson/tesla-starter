namespace TeslaStarter.Application.Common.Interfaces;

public interface IDescopeAuthService
{
    Task<DescopeSessionResult?> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default);
}

public class DescopeSessionResult
{
    public Dictionary<string, object>? Claims { get; set; }
}

public class DescopeUser
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class SelectTenantResult
{
    public string SessionToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
