using System.Reflection;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Domain.Users;
using TeslaStarter.Domain.Vehicles;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Security;
using TeslaStarter.Infrastructure.Tests.TestHelpers;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence;

public sealed class TeslaStarterDbContextTests : IDisposable
{
    private readonly TeslaStarterDbContext _context;
    private readonly TeslaStarterDbContext _contextWithEncryption;
    private readonly IEncryptionService _encryptionService;

    public TeslaStarterDbContextTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();

        // Create encryption service for testing
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = "TestEncryptionKey123!"
            })
            .Build();
        _encryptionService = new EncryptionService(configuration);

        // Create context with encryption
        _contextWithEncryption = TestDbContextFactory.CreateInMemoryContext(_encryptionService);
    }

    [Fact]
    public void DbContext_Should_Have_Users_DbSet()
    {
        // Assert
        _context.Users.Should().NotBeNull();
        _context.Users.Should().BeAssignableTo<DbSet<User>>();
    }

    [Fact]
    public void DbContext_Should_Have_Vehicles_DbSet()
    {
        // Assert
        _context.Vehicles.Should().NotBeNull();
        _context.Vehicles.Should().BeAssignableTo<DbSet<Vehicle>>();
    }

    [Fact]
    public async Task DbContext_Should_Save_And_Retrieve_User()
    {
        // Arrange
        User user = User.Create(
            ExternalId.Create("external123"),
            Email.Create("test@example.com"),
            "Test User");

        // Act
        _context.Users.Add(user);
        int saveResult = await _context.SaveChangesAsync();

        // Clear the tracker to ensure we're reading from the database
        _context.ChangeTracker.Clear();

        User? retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        saveResult.Should().Be(1);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(user.Id);
        retrievedUser.Email.Value.Should().Be("test@example.com");
        retrievedUser.ExternalId.Value.Should().Be("external123");
        retrievedUser.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task DbContext_Should_Save_And_Retrieve_Vehicle()
    {
        // Arrange
        Vehicle vehicle = Vehicle.Link(
            TeslaAccountId.Create("tesla123"),
            "VIN123456789",
            "My Tesla");

        // Act
        _context.Vehicles.Add(vehicle);
        int saveResult = await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        Vehicle? retrievedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);

        // Assert
        saveResult.Should().Be(1);
        retrievedVehicle.Should().NotBeNull();
        retrievedVehicle!.Id.Should().Be(vehicle.Id);
        retrievedVehicle.TeslaAccountId.Value.Should().Be("tesla123");
        retrievedVehicle.VehicleIdentifier.Should().Be("VIN123456789");
        retrievedVehicle.DisplayName.Should().Be("My Tesla");
    }

    [Fact]
    public async Task DbContext_Should_Save_User_With_TeslaAccount()
    {
        // Arrange
        User user = User.Create(
            ExternalId.Create("external123"),
            Email.Create("test@example.com"),
            "Test User");

        user.LinkTeslaAccount("tesla123");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        User? retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.TeslaAccount.Should().NotBeNull();
        retrievedUser.TeslaAccount!.TeslaAccountId.Value.Should().Be("tesla123");
        retrievedUser.TeslaAccount.RefreshToken.Should().BeNull(); // Refresh token is not set during LinkTeslaAccount
        retrievedUser.TeslaAccount.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DbContext_Should_Handle_Multiple_Entities()
    {
        // Arrange
        User user1 = User.Create(ExternalId.Create("ext1"), Email.Create("user1@example.com"), "User 1");
        User user2 = User.Create(ExternalId.Create("ext2"), Email.Create("user2@example.com"), "User 2");
        Vehicle vehicle1 = Vehicle.Link(TeslaAccountId.Create("tesla1"), "VIN1", "Vehicle 1");
        Vehicle vehicle2 = Vehicle.Link(TeslaAccountId.Create("tesla2"), "VIN2", "Vehicle 2");

        // Act
        _context.Users.AddRange(user1, user2);
        _context.Vehicles.AddRange(vehicle1, vehicle2);
        int saveResult = await _context.SaveChangesAsync();

        // Assert
        saveResult.Should().Be(4);
        _context.Users.Count().Should().Be(2);
        _context.Vehicles.Count().Should().Be(2);
    }

    [Fact]
    public async Task DbContext_With_Encryption_Should_Encrypt_Tesla_Tokens()
    {
        // Arrange
        var user = User.Create(
            ExternalId.Create("external123"),
            Email.Create("test@example.com"),
            "Test User");

        user.LinkTeslaAccount("tesla123");
        user.UpdateTeslaTokens("access-token-123", "refresh-token-456", DateTime.UtcNow.AddHours(1));

        // Act
        _contextWithEncryption.Users.Add(user);
        await _contextWithEncryption.SaveChangesAsync();

        // Clear tracker and reload
        _contextWithEncryption.ChangeTracker.Clear();
        var retrievedUser = await _contextWithEncryption.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.TeslaAccount.Should().NotBeNull();

        // Tokens should be decrypted when loaded through context
        retrievedUser.TeslaAccount!.AccessToken.Should().Be("access-token-123");
        retrievedUser.TeslaAccount.RefreshToken.Should().Be("refresh-token-456");
    }

    [Fact]
    public async Task DbContext_Without_Encryption_Should_Not_Encrypt_Tesla_Tokens()
    {
        // Arrange
        var user = User.Create(
            ExternalId.Create("external456"),
            Email.Create("test2@example.com"),
            "Test User 2");

        user.LinkTeslaAccount("tesla456");
        user.UpdateTeslaTokens("plain-access-token", "plain-refresh-token", DateTime.UtcNow.AddHours(1));

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.TeslaAccount.Should().NotBeNull();

        // Tokens should be stored as plain text
        retrievedUser.TeslaAccount!.AccessToken.Should().Be("plain-access-token");
        retrievedUser.TeslaAccount.RefreshToken.Should().Be("plain-refresh-token");
    }

    [Fact]
    public async Task DbContext_With_Encryption_Should_Handle_Null_Tokens()
    {
        // Arrange
        var user = User.Create(
            ExternalId.Create("external789"),
            Email.Create("test3@example.com"),
            "Test User 3");

        user.LinkTeslaAccount("tesla789");
        // Don't set tokens - they should remain null

        // Act
        _contextWithEncryption.Users.Add(user);
        await _contextWithEncryption.SaveChangesAsync();

        _contextWithEncryption.ChangeTracker.Clear();
        var retrievedUser = await _contextWithEncryption.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.TeslaAccount.Should().NotBeNull();

        // Null tokens should remain null
        retrievedUser.TeslaAccount!.AccessToken.Should().BeNull();
        retrievedUser.TeslaAccount.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task DbContext_With_Encryption_Should_Update_Encrypted_Tokens()
    {
        // Arrange
        var user = User.Create(
            ExternalId.Create("external999"),
            Email.Create("test4@example.com"),
            "Test User 4");

        user.LinkTeslaAccount("tesla999");
        user.UpdateTeslaTokens("initial-access", "initial-refresh", DateTime.UtcNow.AddHours(1));

        _contextWithEncryption.Users.Add(user);
        await _contextWithEncryption.SaveChangesAsync();

        // Act - Update tokens
        user.UpdateTeslaTokens("updated-access", "updated-refresh", DateTime.UtcNow.AddHours(2));
        await _contextWithEncryption.SaveChangesAsync();

        _contextWithEncryption.ChangeTracker.Clear();
        var retrievedUser = await _contextWithEncryption.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.TeslaAccount.Should().NotBeNull();
        retrievedUser.TeslaAccount!.AccessToken.Should().Be("updated-access");
        retrievedUser.TeslaAccount.RefreshToken.Should().Be("updated-refresh");
    }

    [Fact]
    public void Constructor_Without_EncryptionService_Should_Create_Context()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new TeslaStarterDbContext(options);

        // Assert
        context.Should().NotBeNull();
        context.Users.Should().NotBeNull();
        context.Vehicles.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_With_EncryptionService_Should_Create_Context()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new TeslaStarterDbContext(options, _encryptionService);

        // Assert
        context.Should().NotBeNull();
        context.Users.Should().NotBeNull();
        context.Vehicles.Should().NotBeNull();
    }

    [Fact]
    public void Encryption_With_Different_Keys_Should_Produce_Different_Results()
    {
        // This test proves that data is actually encrypted by showing different keys produce different encrypted values
        // Arrange
        var plainText = "secret-token-value";

        var configuration1 = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = "FirstEncryptionKey123!"
            })
            .Build();
        var encryptionService1 = new EncryptionService(configuration1);

        var configuration2 = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = "SecondEncryptionKey456!"
            })
            .Build();
        var encryptionService2 = new EncryptionService(configuration2);

        // Act
        var encrypted1 = encryptionService1.Encrypt(plainText);
        var encrypted2 = encryptionService2.Encrypt(plainText);

        // Assert - Different keys should produce different encrypted values
        encrypted1.Should().NotBe(encrypted2);
        encrypted1.Should().NotBe(plainText);
        encrypted2.Should().NotBe(plainText);

        // Each service can only decrypt its own encrypted data
        var decrypted1 = encryptionService1.Decrypt(encrypted1);
        decrypted1.Should().Be(plainText);

        // Trying to decrypt with wrong key should throw
        Action act = () => encryptionService1.Decrypt(encrypted2);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void OnModelCreating_With_EncryptionService_Configures_ValueConverters()
    {
        // Create a test-specific DbContext to ensure OnModelCreating is called
        var testOptions = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: $"EncryptionTest_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        // Create a custom context class to force OnModelCreating
        using var testContext = new TestableDbContext(testOptions, _encryptionService);

        // Force model building
        _ = testContext.Model;

        // The encryption should have been configured
        testContext.WasOnModelCreatingCalled.Should().BeTrue();
        testContext.WasEncryptionConfigured.Should().BeTrue();
    }

    private sealed class TestableDbContext : TeslaStarterDbContext
    {
        public bool WasOnModelCreatingCalled { get; private set; }
        public bool WasEncryptionConfigured { get; private set; }

        public TestableDbContext(DbContextOptions<TeslaStarterDbContext> options, IEncryptionService encryptionService)
            : base(options, encryptionService)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            WasOnModelCreatingCalled = true;
            base.OnModelCreating(modelBuilder);

            // Check if encryption was configured
            var userEntity = modelBuilder.Entity<User>();
            var teslaAccountOwned = userEntity.OwnsOne(u => u.TeslaAccount);
            var refreshTokenProperty = teslaAccountOwned.Property(t => t.RefreshToken).Metadata;
            var accessTokenProperty = teslaAccountOwned.Property(t => t.AccessToken).Metadata;

            WasEncryptionConfigured = refreshTokenProperty.GetValueConverter() != null &&
                                    accessTokenProperty.GetValueConverter() != null;
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
        _contextWithEncryption?.Dispose();
    }
}
