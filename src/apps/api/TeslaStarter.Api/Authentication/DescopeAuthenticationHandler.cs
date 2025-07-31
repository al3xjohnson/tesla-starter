using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;

namespace TeslaStarter.Api.Authentication;

internal sealed class DescopeAuthenticationHandler(
    IOptionsMonitor<DescopeAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceProvider serviceProvider)
    : AuthenticationHandler<DescopeAuthenticationOptions>(options, logger, encoder)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;

        string authorizationHeader = Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorizationHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Invalid token");
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            // Create a scope for resolving scoped services
            using IServiceScope scope = _serviceProvider.CreateScope();
            IDescopeAuthService descopeAuthService = scope.ServiceProvider.GetRequiredService<IDescopeAuthService>();
            IUserRepository? userRepository = scope.ServiceProvider.GetService<IUserRepository>();

            // Validate token and get session result with tenants
            DescopeSessionResult? sessionResult = await descopeAuthService.ValidateSessionAsync(token);
            if (sessionResult == null || sessionResult.Claims == null)
            {
                return AuthenticateResult.Fail("Invalid token");
            }

            // Extract Descope user ID
            string? descopeUserId = sessionResult.Claims.TryGetValue("sub", out object? value)
                ? value?.ToString()
                : null;

            if (string.IsNullOrEmpty(descopeUserId))
            {
                return AuthenticateResult.Fail("Invalid token - no user identifier");
            }

            // Create claims list
            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, descopeUserId)
            ];

            // Add standard claims
            if (sessionResult.Claims.TryGetValue("email", out object? emailValue))
                claims.Add(new Claim(ClaimTypes.Email, emailValue.ToString() ?? ""));

            if (sessionResult.Claims.TryGetValue("name", out object? nameValue))
                claims.Add(new Claim(ClaimTypes.Name, nameValue.ToString() ?? ""));

            // Add other custom claims from Descope
            foreach (KeyValuePair<string, object> kvp in sessionResult.Claims)
            {
                if (kvp.Key != "sub" && kvp.Key != "email" && kvp.Key != "name")
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
            }

            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);

            AuthenticationTicket ticket = new(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating token");
            return AuthenticateResult.Fail("Token validation failed");
        }
    }
}

internal sealed class DescopeAuthenticationOptions : AuthenticationSchemeOptions
{
    public static string Scheme => "Descope";
}
