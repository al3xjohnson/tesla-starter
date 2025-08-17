using System.Security.Claims;

namespace TeslaStarter.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetDescopeUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

