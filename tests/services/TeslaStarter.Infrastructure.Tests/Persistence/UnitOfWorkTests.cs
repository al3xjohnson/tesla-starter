using Common.Domain.Events;
using Common.Domain.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TeslaStarter.Domain.Users;
using TeslaStarter.Domain.Users.Events;
using TeslaStarter.Infrastructure.Persistence;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Persistence;

public sealed class UnitOfWorkTests : IDisposable
{
    private readonly DbContextOptions<TeslaStarterDbContext> _options;
    private readonly TeslaStarterDbContext _context;
    private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _options = new DbContextOptionsBuilder<TeslaStarterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TeslaStarterDbContext(_options);
        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _unitOfWork = new UnitOfWork(_context, _domainEventDispatcherMock.Object);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Save_Changes_To_Database()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        // Act
        int result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        _context.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Dispatch_Domain_Events()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        List<IDomainEvent> capturedEvents = null!;
        _domainEventDispatcherMock
            .Setup(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) => capturedEvents = [.. events])
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _domainEventDispatcherMock.Verify(
            x => x.DispatchEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        capturedEvents.Should().NotBeNull();
        capturedEvents.Should().HaveCount(1);
        capturedEvents.First().Should().BeOfType<UserCreatedDomainEvent>();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Clear_Domain_Events_After_Dispatch()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_With_No_DomainEventDispatcher_Should_Not_Throw()
    {
        // Arrange
        using TeslaStarterDbContext context = new TeslaStarterDbContext(_options);
        using UnitOfWork unitOfWorkWithoutDispatcher = new UnitOfWork(context, null);

        User user = User.Create("external123", "test@example.com", "Test User");
        context.Users.Add(user);

        // Act
        Func<Task> act = async () => await unitOfWorkWithoutDispatcher.SaveChangesAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_With_AcceptAllChangesOnSuccess_False_Should_Pass_Parameter()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        // Act
        int result = await _unitOfWork.SaveChangesAsync(acceptAllChangesOnSuccess: false);

        // Assert
        result.Should().Be(1);
        _context.Users.Count().Should().Be(1);
    }

    [Fact]
    public void HasChanges_Should_Return_True_When_Changes_Exist()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        // Act
        bool hasChanges = _unitOfWork.HasChanges();

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_Should_Return_False_When_No_Changes()
    {
        // Act
        bool hasChanges = _unitOfWork.HasChanges();

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Clear_Should_Clear_Change_Tracker()
    {
        // Arrange
        User user = User.Create("external123", "test@example.com", "Test User");
        _context.Users.Add(user);

        // Act
        _unitOfWork.Clear();

        // Assert
        _unitOfWork.HasChanges().Should().BeFalse();
        _context.Entry(user).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Context_Is_Null()
    {
        // Act
        Action act = () => new UnitOfWork(null!, _domainEventDispatcherMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void Dispose_Should_Dispose_Context()
    {
        // Arrange
        TeslaStarterDbContext context = new TeslaStarterDbContext(_options);
        UnitOfWork unitOfWork = new UnitOfWork(context);

        // Act
        unitOfWork.Dispose();

        // Assert
        Action act = () => context.Users.Count();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_Multiple_Times_Should_Not_Throw()
    {
        // Arrange
        TeslaStarterDbContext context = new TeslaStarterDbContext(_options);
        UnitOfWork unitOfWork = new UnitOfWork(context);

        // Act
        Action act = () =>
        {
            unitOfWork.Dispose();
            unitOfWork.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
