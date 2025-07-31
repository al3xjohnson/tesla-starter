# ADR-002: Testing Strategy and Framework Selection

## Date
2025-07-22

## Status
Proposed

## Context
The TeslaStarter project requires a comprehensive testing strategy to ensure code quality, maintainability, and reliability. We need to establish testing frameworks, patterns, and best practices for both the .NET backend and React frontend.

## Decision

### .NET Testing Stack
- **Framework**: XUnit 2.x
- **Mocking**: Moq 4.x
- **Assertions**: FluentAssertions
- **Test Data**: Bogus (Faker.NET)
- **Integration Testing**: ASP.NET Core TestServer
- **Code Coverage**: Coverlet

### Frontend Testing Stack
- **Unit Testing**: Vitest
- **Component Testing**: React Testing Library
- **E2E Testing**: Playwright
- **Mocking**: MSW (Mock Service Worker)

## Test Organization

### .NET Test Structure
```
tests/
├── api/
│   └── TeslaStarter.Api.Tests/
│       ├── Controllers/          # Controller unit tests
│       ├── Integration/          # API integration tests
│       ├── Fixtures/            # Test fixtures and helpers
│       └── TestData/            # Test data builders
└── services/
    ├── TeslaStarter.Domain.Tests/
    │   ├── Entities/            # Domain entity tests
    │   ├── ValueObjects/        # Value object tests
    │   └── Validators/          # Domain validation tests
    ├── TeslaStarter.Core.Tests/
    │   ├── UseCases/            # Use case/handler tests
    │   ├── Services/            # Business service tests
    │   └── Mappers/             # Mapping tests
    └── TeslaStarter.Infrastructure.Tests/
        ├── TeslaApi/            # Tesla API client tests
        ├── Repositories/        # Repository tests
        └── External/            # External service tests
```

## Testing Principles

### 1. Test Naming Convention
```csharp
public class CalculateTeslaStarterHealthTests
{
    [Fact]
    public void Execute_WhenBatteryLevelIsLow_ReturnsUnhealthyStatus()
    {
        // Test implementation
    }
}
```
Pattern: `MethodName_StateUnderTest_ExpectedBehavior`

### 2. AAA Pattern (Arrange, Act, Assert)
```csharp
[Fact]
public void UpdateMood_WhenEfficiencyIsHigh_ReturnHappyMood()
{
    // Arrange
    TeslaStarterBuilder teslastarter = new()
        .WithEfficiency(95)
        .Build();
    MoodService service = new();

    // Act
    MoodUpdateResult result = service.UpdateMood(teslastarter);

    // Assert
    result.Mood.Should().Be(MoodType.Happy);
}
```

### 3. Test Data Builders
```csharp
public class TeslaStarterBuilder
{
    private string _name = "TestGotchi";
    private int _level = 1;
    private decimal _efficiency = 85;

    public TeslaStarterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TeslaStarterBuilder WithEfficiency(decimal efficiency)
    {
        _efficiency = efficiency;
        return this;
    }

    public TeslaStarter Build()
    {
        return new TeslaStarter(_name, _level, _efficiency);
    }
}
```

### 4. Integration Test Base Class
```csharp
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with test doubles
                services.RemoveAll<ITeslaApiClient>();
                services.AddSingleton<ITeslaApiClient, MockTeslaApiClient>();
            });
        }).CreateClient();

        Scope = factory.Services.CreateScope();
    }
}
```

### 5. Test Categories
- **Unit Tests**: Fast, isolated, no external dependencies
- **Integration Tests**: Test component interactions, may use TestServer
- **E2E Tests**: Full system tests including database and external services

## Best Practices

### 1. Test Isolation
- Each test should be independent
- Use fresh test data for each test
- Clean up resources in test disposal

### 2. Mock External Dependencies
```csharp
[Fact]
public async Task GetVehicleData_WhenApiCallSucceeds_ReturnsMappedData()
{
    // Arrange
    Mock<ITeslaApiClient> mockTeslaApi = new();
    mockTeslaApi.Setup(x => x.GetVehicleDataAsync(It.IsAny<string>()))
        .ReturnsAsync(new VehicleData { BatteryLevel = 80 });

    VehicleService service = new(mockTeslaApi.Object);

    // Act
    VehicleData result = await service.GetVehicleDataAsync("123");

    // Assert
    result.BatteryLevel.Should().Be(80);
    mockTeslaApi.Verify(x => x.GetVehicleDataAsync("123"), Times.Once);
}
```

### 3. Parameterized Tests
```csharp
[Theory]
[InlineData(90, 100, MoodType.Happy)]
[InlineData(50, 70, MoodType.Content)]
[InlineData(20, 30, MoodType.Sad)]
public void CalculateMood_WithDifferentMetrics_ReturnsExpectedMood(
    int efficiency, int batteryLevel, MoodType expectedMood)
{
    // Test implementation
}
```

### 4. Test Coverage Goals
- Minimum 80% code coverage for business logic
- 100% coverage for critical paths (payment, authentication)
- Focus on behavior coverage over line coverage

## Testing Environments

### 1. Local Development
- In-memory database for fast feedback
- Docker containers for external dependencies
- Test data seeding scripts

### 2. CI/CD Pipeline
- Run all unit tests on every commit
- Run integration tests on PR creation
- Run E2E tests before deployment

### 3. Test Data Management
- Use factories for consistent test data
- Separate test data from production
- Anonymized production data for load testing

## Consequences

### Benefits
- **XUnit**: Modern, extensible, excellent async support
- **FluentAssertions**: Readable test assertions
- **Test Isolation**: Reliable, repeatable tests
- **Builder Pattern**: Flexible test data creation
- **Integration Tests**: Confidence in component interactions

### Trade-offs
- Initial setup complexity
- Learning curve for team members new to XUnit
- Maintenance overhead for test builders
- Slower CI/CD pipeline with comprehensive tests

## References
- [XUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Testing Documentation](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [Test Data Builder Pattern](https://www.natpryce.com/articles/000714.html)