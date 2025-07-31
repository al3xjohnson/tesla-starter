using Descope;
using TeslaStarter.Application.Common.Interfaces;

namespace TeslaStarter.Infrastructure.Authentication;

public class DescopeAuthService(DescopeClient descopeClient) : IDescopeAuthService
{
    private readonly DescopeClient _descopeClient = descopeClient;

    public async Task<DescopeSessionResult?> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        Token result = await _descopeClient.Auth.ValidateSession(sessionToken);
        if (result == null)
            return null;

        return new DescopeSessionResult
        {
            Claims = result.Claims
        };
    }
}
