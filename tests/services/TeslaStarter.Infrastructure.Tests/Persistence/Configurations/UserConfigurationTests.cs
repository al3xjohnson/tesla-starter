using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TeslaStarter.Domain.Users;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Tests.TestHelpers;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence.Configurations;

public sealed class UserConfigurationTests : IDisposable
{
    private readonly TeslaStarterDbContext _context;

    public UserConfigurationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
    }

    [Fact]
    public void UserConfiguration_Should_Configure_Table_Name()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("users");
    }

    [Fact]
    public void UserConfiguration_Should_Configure_Primary_Key()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));
        IKey? primaryKey = entityType!.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void UserConfiguration_Should_Configure_Properties()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));

        // Assert
        IProperty? idProperty = entityType!.FindProperty("Id");
        idProperty.Should().NotBeNull();
        idProperty!.IsNullable.Should().BeFalse();

        IProperty? externalIdProperty = entityType!.FindProperty("ExternalId");
        externalIdProperty.Should().NotBeNull();
        externalIdProperty!.IsNullable.Should().BeFalse();
        externalIdProperty.GetMaxLength().Should().Be(255);

        IProperty? emailProperty = entityType!.FindProperty("Email");
        emailProperty.Should().NotBeNull();
        emailProperty!.IsNullable.Should().BeFalse();
        emailProperty.GetMaxLength().Should().Be(255);

        IProperty? displayNameProperty = entityType!.FindProperty("DisplayName");
        displayNameProperty.Should().NotBeNull();
        displayNameProperty!.GetMaxLength().Should().Be(100);

        IProperty? createdAtProperty = entityType!.FindProperty("CreatedAt");
        createdAtProperty.Should().NotBeNull();
        createdAtProperty!.IsNullable.Should().BeFalse();

        IProperty? lastLoginAtProperty = entityType!.FindProperty("LastLoginAt");
        lastLoginAtProperty.Should().NotBeNull();
        lastLoginAtProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_Should_Configure_TeslaAccount_As_Owned()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));
        INavigation? navigation = entityType!.FindNavigation("TeslaAccount");

        // Assert
        navigation.Should().NotBeNull();
        navigation!.TargetEntityType.IsOwned().Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_Should_Configure_Indexes()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));
        IEnumerable<IIndex> indexes = [.. entityType!.GetIndexes()];

        // Assert
        IIndex? externalIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "ExternalId"));
        externalIdIndex.Should().NotBeNull();
        externalIdIndex!.IsUnique.Should().BeTrue();

        IIndex? emailIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Email"));
        emailIndex.Should().NotBeNull();
    }

    [Fact]
    public void UserConfiguration_Should_Ignore_DomainEvents()
    {
        // Arrange & Act
        IEntityType? entityType = _context.Model.FindEntityType(typeof(User));
        IProperty? domainEventsProperty = entityType!.FindProperty("DomainEvents");

        // Assert
        domainEventsProperty.Should().BeNull();
    }

    [Fact]
    public async Task UserConfiguration_Should_Save_And_Retrieve_Value_Objects()
    {
        // Arrange
        User user = User.Create(
            ExternalId.Create("ext123"),
            Email.Create("test@example.com"),
            "Test User");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        User savedUser = await _context.Users.FirstAsync();

        // Assert
        savedUser.ExternalId.Value.Should().Be("ext123");
        savedUser.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task UserConfiguration_Should_Handle_TeslaAccount_Properly()
    {
        // Arrange
        User user = User.Create(
            ExternalId.Create("ext123"),
            Email.Create("test@example.com"),
            "Test User");

        user.LinkTeslaAccount("tesla123");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        User savedUser = await _context.Users.FirstAsync();

        // Assert
        savedUser.TeslaAccount.Should().NotBeNull();
        savedUser.TeslaAccount!.TeslaAccountId.Value.Should().Be("tesla123");
        savedUser.TeslaAccount.RefreshToken.Should().BeNull(); // Refresh token is not set during LinkTeslaAccount
        savedUser.TeslaAccount.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
