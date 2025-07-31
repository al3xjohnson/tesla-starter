using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using TeslaStarter.Infrastructure.DependencyInjection;
using TeslaStarter.Infrastructure.Persistence;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_Should_Register_DbContext()
    {
        // Arrange
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
            ["Encryption:Key"] = "test-encryption-key-for-unit-tests"
        });
        IHostEnvironment environment = CreateEnvironment(isDevelopment: false);

        // Add IConfiguration to services
        services.AddSingleton(configuration);

        // Act
        services.AddInfrastructure(configuration, environment);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        TeslaStarterDbContext? dbContext = serviceProvider.GetService<TeslaStarterDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_Should_Register_DbContext_With_Scoped_Lifetime()
    {
        // Arrange
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
        });
        IHostEnvironment environment = CreateEnvironment(isDevelopment: false);

        // Act
        services.AddInfrastructure(configuration, environment);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TeslaStarterDbContext));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_Should_Return_ServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
        });
        IHostEnvironment environment = CreateEnvironment(isDevelopment: false);

        // Act
        IServiceCollection result = services.AddInfrastructure(configuration, environment);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructure_Should_Work_In_Development_Environment()
    {
        // Arrange
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
            ["Encryption:Key"] = "test-encryption-key-for-unit-tests"
        });
        IHostEnvironment environment = CreateEnvironment(isDevelopment: true);

        // Add IConfiguration to services
        services.AddSingleton(configuration);

        // Act
        services.AddInfrastructure(configuration, environment);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        TeslaStarterDbContext? dbContext = serviceProvider.GetService<TeslaStarterDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_Should_Work_In_Production_Environment()
    {
        // Arrange
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
            ["Encryption:Key"] = "test-encryption-key-for-unit-tests"
        });
        IHostEnvironment environment = CreateEnvironment(isDevelopment: false);

        // Add IConfiguration to services
        services.AddSingleton(configuration);

        // Act
        services.AddInfrastructure(configuration, environment);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        TeslaStarterDbContext? dbContext = serviceProvider.GetService<TeslaStarterDbContext>();
        dbContext.Should().NotBeNull();
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IHostEnvironment CreateEnvironment(bool isDevelopment)
    {
        Mock<IHostEnvironment> mock = new();
        mock.Setup(e => e.EnvironmentName)
            .Returns(isDevelopment ? "Development" : "Production");
        mock.Setup(e => e.ApplicationName)
            .Returns("TeslaStarter.Api");
        mock.Setup(e => e.ContentRootPath)
            .Returns("/app");
        return mock.Object;
    }
}
