using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;
using TeslaStarter.Domain.Vehicles;
using TeslaStarter.Infrastructure.Authentication;
using TeslaStarter.Infrastructure.Configuration;
using TeslaStarter.Infrastructure.ExternalServices;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Persistence.Repositories;
using TeslaStarter.Infrastructure.Security;

namespace TeslaStarter.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Security Services
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Database Configuration
        services.AddDbContext<TeslaStarterDbContext>((serviceProvider, options) =>
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null));

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register DbContext factory that includes encryption service
        services.AddScoped<TeslaStarterDbContext>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<TeslaStarterDbContext>>();
            var encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();
            return new TeslaStarterDbContext(options, encryptionService);
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();

        // Unit of Work
        services.AddScoped<Common.Domain.Persistence.IUnitOfWork, UnitOfWork>();

        // Configuration
        services.Configure<TeslaOptions>(configuration.GetSection(TeslaOptions.SectionName));

        // External Services
        services.AddHttpClient<ITeslaOAuthService, TeslaOAuthService>();
        services.AddHttpClient<ITeslaApiService, TeslaApiService>();

        return services;
    }
}
