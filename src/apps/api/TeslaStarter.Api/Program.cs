using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TeslaStarter.Api.Extensions;
using TeslaStarter.Infrastructure.Persistence;

// Configure Serilog
#pragma warning disable CA1305 // Specify IFormatProvider
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
#pragma warning restore CA1305 // Specify IFormatProvider

try
{
    Log.Information("Starting TeslaStarter API");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddApiServices(builder.Configuration, builder.Environment);

    WebApplication app = builder.Build();

    // Apply database migrations automatically in development
    bool skipMigration = app.Configuration.GetValue<bool>("SkipDatabaseMigration");
    if (!skipMigration && app.Environment.IsDevelopment())
    {
        using IServiceScope scope = app.Services.CreateScope();
        try
        {
            TeslaStarterDbContext dbContext = scope.ServiceProvider.GetRequiredService<TeslaStarterDbContext>();
            app.Logger.LogInformation("Checking database connection...");

            // Migrate will create the database if it doesn't exist AND apply all migrations
            app.Logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    // Configure middleware pipeline
    app.ConfigureMiddleware();

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        // Attach additional properties to the request completion event
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown");
        };
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
