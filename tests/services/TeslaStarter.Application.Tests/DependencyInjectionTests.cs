namespace TeslaStarter.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_RegistersAutoMapper()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IMapper? mapper = provider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Verify configuration is valid by checking the mapper works
        User testUser = User.Create("ext123", "test@example.com", "Test User");
        Action action = () => mapper!.Map<UserDto>(testUser);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddApplicationServices_RegistersFluentValidation()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // Check that validators are registered
        IValidator<CreateUserCommand>? createUserValidator = provider.GetService<IValidator<CreateUserCommand>>();
        createUserValidator.Should().NotBeNull();

        IValidator<UpdateProfileCommand>? updateProfileValidator = provider.GetService<IValidator<UpdateProfileCommand>>();
        updateProfileValidator.Should().NotBeNull();

        IValidator<LinkTeslaAccountCommand>? linkTeslaAccountValidator = provider.GetService<IValidator<LinkTeslaAccountCommand>>();
        linkTeslaAccountValidator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_RegistersMediatR()
    {
        // Arrange
        ServiceCollection services = new();

        // Add required dependencies for MediatR handlers
        services.AddScoped(_ => Mock.Of<IUserRepository>());
        services.AddScoped(_ => Mock.Of<IVehicleRepository>());
        services.AddScoped(_ => Mock.Of<IUnitOfWork>());
        services.AddScoped(typeof(ILogger<>), typeof(MockLogger<>));

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IMediator? mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        // Verify a handler is registered
        IRequestHandler<CreateUserCommand, UserDto>? handler = provider.GetService<IRequestHandler<CreateUserCommand, UserDto>>();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_RegistersValidationBehavior()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IEnumerable<IPipelineBehavior<CreateUserCommand, UserDto>> behaviors = provider.GetServices<IPipelineBehavior<CreateUserCommand, UserDto>>();
        behaviors.Should().NotBeNull();
        behaviors.Should().Contain(b => b.GetType().IsGenericType &&
            b.GetType().GetGenericTypeDefinition() == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddApplicationServices_RegistersAllValidators()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Check all command validators are registered
        provider.GetService<IValidator<CreateUserCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<UpdateProfileCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<LinkTeslaAccountCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<UnlinkTeslaAccountCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<LinkVehicleCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<UpdateVehicleCommand>>().Should().NotBeNull();
        provider.GetService<IValidator<UnlinkVehicleCommand>>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_RegistersAllMappingProfiles()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IMapper mapper = provider.GetRequiredService<IMapper>();

        // Test User mapping
        User user = User.Create("ext123", "test@example.com", "Test");
        UserDto userDto = mapper.Map<UserDto>(user);
        userDto.Should().NotBeNull();

        // Test Vehicle mapping
        Vehicle vehicle = Vehicle.Link(TeslaAccountId.Create("tesla123"), "VIN123", "Tesla");
        VehicleDto vehicleDto = mapper.Map<VehicleDto>(vehicle);
        vehicleDto.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_ReturnsServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddApplicationServices();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddApplicationServices_RegistersFromCorrectAssembly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddApplicationServices();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // Verify that types from Application assembly are registered
        System.Reflection.Assembly applicationAssembly = typeof(DependencyInjection).Assembly;

        // Check that a type from the Application assembly is registered
        List<System.Reflection.Assembly> registeredTypes = services
            .Where(s => s.ImplementationType != null)
            .Select(s => s.ImplementationType!.Assembly)
            .Distinct()
            .ToList();

        registeredTypes.Should().Contain(applicationAssembly);
    }

    // Mock logger for testing
    private sealed class MockLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
