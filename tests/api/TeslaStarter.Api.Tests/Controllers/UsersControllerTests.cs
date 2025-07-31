using TeslaStarter.Api.Controllers;

namespace TeslaStarter.Api.Tests.Controllers;

public sealed class UsersControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task GetUser_ValidId_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserDto expectedUser = new()
        {
            Id = userId,
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUser_NotFound_ReturnsNotFound()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException("User", userId));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task GetUserByExternalId_ValidId_ReturnsOk()
    {
        // Arrange
        string externalId = "ext123";
        UserDto expectedUser = new()
        {
            Id = Guid.NewGuid(),
            ExternalId = externalId,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/external/{externalId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task CreateUser_ValidCommand_ReturnsCreated()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        UserDto expectedUser = new()
        {
            Id = Guid.NewGuid(),
            ExternalId = command.ExternalId,
            Email = command.Email,
            DisplayName = command.DisplayName
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        // No authorization header required for CreateUser

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"api/v1/users/{expectedUser.Id}");
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task CreateUser_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            ExternalId = "", // Invalid
            Email = "invalid-email", // Invalid
            DisplayName = "Test User"
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ValidationException(new[]
                    {
                        new FluentValidation.Results.ValidationFailure("ExternalId", "External ID is required"),
                        new FluentValidation.Results.ValidationFailure("Email", "Invalid email format")
                    }));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("One or more validation errors occurred.");
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UpdateProfileRequest request = new()
        {
            Email = "newemail@example.com",
            DisplayName = "Updated Name"
        };

        UserDto expectedUser = new()
        {
            Id = userId,
            ExternalId = "ext123",
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UpdateProfileCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/users/{userId}/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task LinkTeslaAccount_ValidRequest_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        LinkTeslaAccountRequest request = new()
        {
            TeslaAccountId = "tesla123"
        };

        UserDto expectedUser = new()
        {
            Id = userId,
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User",
            TeslaAccount = new TeslaAccountDto
            {
                TeslaAccountId = request.TeslaAccountId,
                IsActive = true
            }
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<LinkTeslaAccountCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/v1/users/{userId}/tesla-account", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task UnlinkTeslaAccount_ValidId_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserDto expectedUser = new()
        {
            Id = userId,
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User",
            TeslaAccount = null
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UnlinkTeslaAccountCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/users/{userId}/tesla-account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task RecordLogin_ValidId_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserDto expectedUser = new()
        {
            Id = userId,
            ExternalId = "ext123",
            Email = "test@example.com",
            DisplayName = "Test User",
            LastLoginAt = DateTime.UtcNow
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<RecordLoginCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/users/{userId}/login", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDto? result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(expectedUser);
    }
}
