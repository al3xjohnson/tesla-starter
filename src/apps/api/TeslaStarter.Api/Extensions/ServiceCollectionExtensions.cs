using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using TeslaStarter.Application;
using TeslaStarter.Infrastructure.DependencyInjection;

namespace TeslaStarter.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add controllers
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Customize model validation response to use ProblemDetails
                options.InvalidModelStateResponseFactory = context =>
                {
                    ValidationProblemDetails problemDetails = new(context.ModelState)
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "One or more validation errors occurred.",
                        Status = StatusCodes.Status400BadRequest,
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

        // Configure API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Configure Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TeslaStarter API",
                Version = "v1",
                Description = "API for managing Tesla virtual pets",
                Contact = new OpenApiContact
                {
                    Name = "TeslaStarter Team",
                    Email = "support@teslastarter.com"
                }
            });

            // Include XML comments for better API documentation
            string xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        // Add Authentication services
        services.AddDescopeAuthentication(configuration);

        // Add Application services (includes MediatR, FluentValidation, AutoMapper)
        services.AddApplicationServices();

        // Add Infrastructure services (includes Entity Framework Core)
        services.AddInfrastructure(configuration, environment);

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", policy =>
            {
                string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else if (environment.IsDevelopment())
                {
                    // Allow any origin in development
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        // Add health checks
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        // Add memory cache (needed for Tesla OAuth state management)
        services.AddMemoryCache();

        return services;
    }
}
