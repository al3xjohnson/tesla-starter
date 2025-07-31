using Descope;
using Microsoft.Extensions.Options;
using TeslaStarter.Api.Authentication;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Infrastructure.Authentication;
using TeslaStarter.Infrastructure.Configuration;

namespace TeslaStarter.Api.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddDescopeAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Descope options
        services.Configure<DescopeOptions>(configuration.GetSection(DescopeOptions.SectionName));

        // Add Descope client
        services.AddSingleton<DescopeClient>(serviceProvider =>
        {
            DescopeOptions descopeOptions = serviceProvider.GetRequiredService<IOptions<DescopeOptions>>().Value;
            DescopeConfig config = new(projectId: descopeOptions.ProjectId)
            {
                ManagementKey = descopeOptions.ManagementKey,
            };
            return new DescopeClient(config);
        });

        // Add memory cache
        services.AddMemoryCache();

        // Add HTTP client for Descope service
        services.AddScoped<IDescopeAuthService, DescopeAuthService>();
        services.AddScoped<IUserSynchronizationService, UserSynchronizationService>();

        // Add HTTP client for Tesla OAuth service
        services.AddHttpClient<ITeslaOAuthService, TeslaOAuthService>();

        // Add authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = DescopeAuthenticationOptions.Scheme;
            options.DefaultChallengeScheme = DescopeAuthenticationOptions.Scheme;
        })
        .AddScheme<DescopeAuthenticationOptions, DescopeAuthenticationHandler>(
            DescopeAuthenticationOptions.Scheme,
            options => { });

        services.AddAuthorization();

        return services;
    }
}
