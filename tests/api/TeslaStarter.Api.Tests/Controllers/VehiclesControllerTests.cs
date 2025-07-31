using TeslaStarter.Api.Controllers;

namespace TeslaStarter.Api.Tests.Controllers;

public sealed class VehiclesControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public VehiclesControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task GetVehicle_ValidId_ReturnsOk()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();
        VehicleDto expectedVehicle = new()
        {
            Id = vehicleId,
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla",
            IsActive = true
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetVehicleQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedVehicle);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/vehicles/{vehicleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VehicleDto? result = await response.Content.ReadFromJsonAsync<VehicleDto>();
        result.Should().BeEquivalentTo(expectedVehicle);
    }

    [Fact]
    public async Task GetVehicle_NotFound_ReturnsNotFound()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetVehicleQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException("Vehicle", vehicleId));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/vehicles/{vehicleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVehiclesByTeslaAccount_ValidAccount_ReturnsOk()
    {
        // Arrange
        string teslaAccountId = "tesla123";
        List<VehicleDto> expectedVehicles =
        [
            new VehicleDto {
                Id = Guid.NewGuid(),
                TeslaAccountId = teslaAccountId,
                VehicleIdentifier = "VIN1",
                DisplayName = "Tesla 1",
                IsActive = true
            },
            new VehicleDto {
                Id = Guid.NewGuid(),
                TeslaAccountId = teslaAccountId,
                VehicleIdentifier = "VIN2",
                DisplayName = "Tesla 2",
                IsActive = true
            }
        ];

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetVehiclesByTeslaAccountQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedVehicles.AsReadOnly());
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/vehicles/tesla-account/{teslaAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<VehicleDto>? result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
        result.Should().BeEquivalentTo(expectedVehicles);
    }

    [Fact]
    public async Task GetVehiclesByTeslaAccount_EmptyList_ReturnsOk()
    {
        // Arrange
        string teslaAccountId = "tesla123";

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<GetVehiclesByTeslaAccountQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<VehicleDto>().AsReadOnly());
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/vehicles/tesla-account/{teslaAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<VehicleDto>? result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task LinkVehicle_ValidCommand_ReturnsCreated()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = "My Tesla"
        };

        VehicleDto expectedVehicle = new()
        {
            Id = Guid.NewGuid(),
            TeslaAccountId = command.TeslaAccountId,
            VehicleIdentifier = command.VehicleIdentifier,
            DisplayName = command.DisplayName,
            IsActive = true
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<LinkVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedVehicle);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/vehicles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"api/v1/vehicles/{expectedVehicle.Id}");
        VehicleDto? result = await response.Content.ReadFromJsonAsync<VehicleDto>();
        result.Should().BeEquivalentTo(expectedVehicle);
    }

    [Fact]
    public async Task LinkVehicle_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        LinkVehicleCommand command = new()
        {
            TeslaAccountId = "", // Invalid
            VehicleIdentifier = "", // Invalid
            DisplayName = "My Tesla"
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<LinkVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ValidationException(new[]
                    {
                        new FluentValidation.Results.ValidationFailure("TeslaAccountId", "Tesla account ID is required"),
                        new FluentValidation.Results.ValidationFailure("VehicleIdentifier", "Vehicle identifier is required")
                    }));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/vehicles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("One or more validation errors occurred.");
    }

    [Fact]
    public async Task UpdateVehicle_ValidRequest_ReturnsOk()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();
        UpdateVehicleRequest request = new()
        {
            DisplayName = "Updated Tesla"
        };

        VehicleDto expectedVehicle = new()
        {
            Id = vehicleId,
            TeslaAccountId = "tesla123",
            VehicleIdentifier = "VIN123",
            DisplayName = request.DisplayName,
            IsActive = true
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UpdateVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedVehicle);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/vehicles/{vehicleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        VehicleDto? result = await response.Content.ReadFromJsonAsync<VehicleDto>();
        result.Should().BeEquivalentTo(expectedVehicle);
    }

    [Fact]
    public async Task UpdateVehicle_NotFound_ReturnsNotFound()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();
        UpdateVehicleRequest request = new()
        {
            DisplayName = "Updated Tesla"
        };

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UpdateVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException("Vehicle", vehicleId));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/vehicles/{vehicleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnlinkVehicle_ValidId_ReturnsNoContent()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UnlinkVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/vehicles/{vehicleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnlinkVehicle_NotFound_ReturnsNotFound()
    {
        // Arrange
        Guid vehicleId = Guid.NewGuid();

        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<ISender> mockMediator = new();
                mockMediator.Setup(x => x.Send(It.IsAny<UnlinkVehicleCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NotFoundException("Vehicle", vehicleId));
                services.AddSingleton(mockMediator.Object);
            });
        });

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/vehicles/{vehicleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
