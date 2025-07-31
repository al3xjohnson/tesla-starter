using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Tests.TestHelpers;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence;

public class MigrationTests
{
    [Fact]
    public void DbContext_Model_Should_Be_Valid()
    {
        // Arrange
        using TeslaStarterDbContext context = TestDbContextFactory.CreateInMemoryContext();

        // Act & Assert
        Microsoft.EntityFrameworkCore.Metadata.IModel model = context.Model;
        model.Should().NotBeNull();

        // Verify model has entity types
        System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IEntityType> entityTypes = model.GetEntityTypes();
        entityTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void DbContext_Should_Have_Migrations()
    {
        // Arrange - Use a real provider to test migrations
        DbContextOptions<TeslaStarterDbContext> options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseNpgsql("Host=fake;Database=fake") // Just for metadata, not actually connecting
            .Options;

        using TeslaStarterDbContext context = new TeslaStarterDbContext(options);

        // Act
        System.Collections.Generic.IReadOnlyDictionary<string, TypeInfo> migrations = context.Database
            .GetService<IMigrationsAssembly>()
            .Migrations;

        // Assert
        migrations.Should().NotBeEmpty();
        migrations.Should().ContainKey("20250730211908_InitialCreate");
    }

    [Fact]
    public void All_Entities_Should_Have_Keys_Configured()
    {
        // Arrange
        using TeslaStarterDbContext context = TestDbContextFactory.CreateInMemoryContext();

        // Act
        System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IEntityType> entityTypes = context.Model.GetEntityTypes()
            .Where(e => !e.IsOwned());

        // Assert
        foreach (Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType in entityTypes)
        {
            Microsoft.EntityFrameworkCore.Metadata.IKey? primaryKey = entityType.FindPrimaryKey();
            primaryKey.Should().NotBeNull($"{entityType.Name} should have a primary key configured");
        }
    }

    [Fact]
    public void All_Required_Properties_Should_Be_Configured()
    {
        // Arrange
        using TeslaStarterDbContext context = TestDbContextFactory.CreateInMemoryContext();

        // Act & Assert
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? userEntity = context.Model.FindEntityType(typeof(Domain.Users.User));
        userEntity.Should().NotBeNull();

        string[] requiredProperties = new[] { "Id", "ExternalId", "Email", "CreatedAt" };
        foreach (string propertyName in requiredProperties)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = userEntity!.FindProperty(propertyName);
            property.Should().NotBeNull($"User.{propertyName} should be configured");
            property!.IsNullable.Should().BeFalse($"User.{propertyName} should not be nullable");
        }

        Microsoft.EntityFrameworkCore.Metadata.IEntityType? vehicleEntity = context.Model.FindEntityType(typeof(Domain.Vehicles.Vehicle));
        vehicleEntity.Should().NotBeNull();

        string[] vehicleRequiredProperties = new[] { "Id", "TeslaAccountId", "VehicleIdentifier", "LinkedAt", "IsActive" };
        foreach (string propertyName in vehicleRequiredProperties)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = vehicleEntity!.FindProperty(propertyName);
            property.Should().NotBeNull($"Vehicle.{propertyName} should be configured");
            property!.IsNullable.Should().BeFalse($"Vehicle.{propertyName} should not be nullable");
        }
    }

    [Fact]
    public void All_Indexes_Should_Be_Configured()
    {
        // Arrange
        using TeslaStarterDbContext context = TestDbContextFactory.CreateInMemoryContext();

        // Act
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? userEntity = context.Model.FindEntityType(typeof(Domain.Users.User));
        System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IIndex> userIndexes = userEntity!.GetIndexes();

        Microsoft.EntityFrameworkCore.Metadata.IEntityType? vehicleEntity = context.Model.FindEntityType(typeof(Domain.Vehicles.Vehicle));
        System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IIndex> vehicleIndexes = vehicleEntity!.GetIndexes();

        // Assert
        userIndexes.Should().NotBeEmpty("User entity should have indexes");
        userIndexes.Should().Contain(i => i.Properties.Any(p => p.Name == "ExternalId"),
            "User should have index on ExternalId");
        userIndexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Email"),
            "User should have index on Email");

        vehicleIndexes.Should().NotBeEmpty("Vehicle entity should have indexes");
        vehicleIndexes.Should().Contain(i => i.Properties.Any(p => p.Name == "TeslaAccountId"),
            "Vehicle should have index on TeslaAccountId");
        vehicleIndexes.Should().Contain(i => i.Properties.Any(p => p.Name == "VehicleIdentifier"),
            "Vehicle should have index on VehicleIdentifier");
    }
}
