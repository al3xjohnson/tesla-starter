using TeslaStarter.Api.Middleware;

namespace TeslaStarter.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        // Global exception middleware should be first to catch all exceptions
        app.UseGlobalExceptionHandling();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeslaStarter API v1");
                options.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
            });
        }

        // Security headers
        app.UseHsts();
        app.UseHttpsRedirection();

        // CORS
        app.UseCors("AllowedOrigins");

        // Static files
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.MapHealthChecks("/health");

        // API endpoints
        app.MapControllers();

        return app;
    }
}
