using Common.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeslaStarter.Domain.Users;
using TeslaStarter.Infrastructure.Persistence;
using TeslaStarter.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence.Repositories;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly TeslaStarterDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TeslaStarterDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Add_ShouldAddUserToContext()
    {
        var user = User.Create("external123", "test@example.com", "Test User");

        _repository.Add(user);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task Update_ShouldUpdateUserInContext()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.UpdateProfile("new@example.com", "New Name");
        _repository.Update(user);
        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.DisplayName.Should().Be("New Name");
        updatedUser.Email.Value.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Remove_ShouldRemoveUserFromContext()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _repository.Remove(user);
        await _context.SaveChangesAsync();

        var removedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        removedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Generic_WithUserId_ShouldReturnUser()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync<UserId>(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Generic_WithNonUserId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync<string>("not-a-userid");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithUserId_ShouldReturnUser()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(UserId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalIdAsync_ShouldReturnUser()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByExternalIdAsync(user.ExternalId);

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be(user.ExternalId);
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _repository.GetByExternalIdAsync(ExternalId.Create("nonexistent"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsAsync(user.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var result = await _repository.ExistsAsync(UserId.New());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByExternalIdAsync_WithExistingId_ShouldReturnTrue()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByExternalIdAsync(user.ExternalId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByExternalIdAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByExternalIdAsync(ExternalId.Create("nonexistent"));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync(user.Email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        var result = await _repository.GetByEmailAsync(Email.Create("nonexistent@example.com"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        var user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByEmailAsync(user.Email);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByEmailAsync(Email.Create("nonexistent@example.com"));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsersWithTeslaAccounts()
    {
        var user1 = User.Create("external1", "test1@example.com", "User 1");
        var user2 = User.Create("external2", "test2@example.com", "User 2");
        var user3 = User.Create("external3", "test3@example.com", "User 3");

        user3.LinkTeslaAccount("tesla3");

        _context.Users.Add(user1);
        _context.Users.Add(user2);
        _context.Users.Add(user3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
        // Should include related TeslaAccount data
        var userWithTesla = result.First(u => u.ExternalId == user3.ExternalId);
        userWithTesla.TeslaAccount.Should().NotBeNull();
        userWithTesla.TeslaAccount!.TeslaAccountId.Value.Should().Be("tesla3");
    }

    [Fact]
    public async Task GetAllAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }
}
