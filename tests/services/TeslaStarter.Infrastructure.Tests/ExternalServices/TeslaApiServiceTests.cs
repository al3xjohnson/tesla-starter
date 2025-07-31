using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Infrastructure.ExternalServices;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.ExternalServices;

public sealed class TeslaApiServiceTests : IDisposable
{
    private readonly Mock<ILogger<TeslaApiService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly TeslaApiService _service;

    public TeslaApiServiceTests()
    {
        _loggerMock = new Mock<ILogger<TeslaApiService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new TeslaApiService(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetVehiclesAsync_WithSuccessfulResponse_ReturnsVehicles()
    {
        // Arrange
        string accessToken = "test-token";
        string responseJson = @"{
            ""response"": [
                {
                    ""Id"": 123456789,
                    ""VehicleId"": 987654321,
                    ""Vin"": ""5YJ3E1EA1JF000001"",
                    ""DisplayName"": ""My Tesla"",
                    ""State"": ""online"",
                    ""OptionCodes"": ""MDL3"",
                    ""Color"": null,
                    ""Tokens"": [""token1"", ""token2""],
                    ""InService"": false,
                    ""CalendarEnabled"": true,
                    ""ApiVersion"": ""12""
                },
                {
                    ""Id"": 234567890,
                    ""VehicleId"": 876543210,
                    ""Vin"": ""5YJ3E1EA1JF000002"",
                    ""DisplayName"": ""Model Y"",
                    ""State"": ""asleep"",
                    ""OptionCodes"": ""MDLY"",
                    ""Color"": ""PEARL_WHITE"",
                    ""Tokens"": [],
                    ""InService"": true,
                    ""CalendarEnabled"": false,
                    ""ApiVersion"": ""12""
                }
            ],
            ""Count"": 2
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == "https://fleet-api.prd.vn.cloud.tesla.com/api/1/vehicles" &&
                    req.Headers.Authorization!.Parameter == accessToken),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().HaveCount(2);

        result[0].Id.Should().Be("123456789");
        result[0].Vin.Should().Be("5YJ3E1EA1JF000001");
        result[0].DisplayName.Should().Be("My Tesla");
        result[0].State.Should().Be("online");

        result[1].Id.Should().Be("234567890");
        result[1].Vin.Should().Be("5YJ3E1EA1JF000002");
        result[1].DisplayName.Should().Be("Model Y");
        result[1].State.Should().Be("asleep");
    }

    [Fact]
    public async Task GetVehiclesAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";
        string responseJson = @"{
            ""response"": [],
            ""count"": 0
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";
        string responseJson = @"{ ""response"": null }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVehiclesAsync_WithMissingFields_HandlesGracefully()
    {
        // Arrange
        string accessToken = "test-token";
        string responseJson = @"{
            ""response"": [
                {
                    ""Id"": 123456789,
                    ""Vin"": null,
                    ""DisplayName"": null,
                    ""State"": null
                }
            ]
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("123456789");
        result[0].Vin.Should().Be("");
        result[0].DisplayName.Should().Be("");
        result[0].State.Should().Be("");
    }

    [Fact]
    public async Task GetVehiclesAsync_WithUnauthorizedResponse_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized")
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch vehicles from Tesla API")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetVehiclesAsync_WithInvalidJson_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json")
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching vehicles from Tesla API")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetVehiclesAsync_WithNetworkError_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching vehicles from Tesla API")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetVehiclesAsync_SetsAuthorizationHeader()
    {
        // Arrange
        string accessToken = "test-bearer-token";
        string responseJson = @"{ ""response"": [], ""count"": 0 }";
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        await _service.GetVehiclesAsync(accessToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be(accessToken);
    }

    [Fact]
    public async Task GetVehiclesAsync_WithCaseInsensitiveJson_DeserializesCorrectly()
    {
        // Arrange
        string accessToken = "test-token";
        // Using different casing for property names
        string responseJson = @"{
            ""Response"": [
                {
                    ""ID"": 123456789,
                    ""VIN"": ""5YJ3E1EA1JF000001"",
                    ""DisplayName"": ""My Tesla"",
                    ""STATE"": ""online""
                }
            ],
            ""COUNT"": 1
        }";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("123456789");
        result[0].Vin.Should().Be("5YJ3E1EA1JF000001");
        result[0].DisplayName.Should().Be("My Tesla");
        result[0].State.Should().Be("online");
    }

    [Fact]
    public async Task GetVehiclesAsync_WithNullDeserializedResponse_ReturnsEmptyList()
    {
        // Arrange
        string accessToken = "test-token";
        // This will deserialize to null
        string responseJson = "null";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        IReadOnlyList<TeslaVehicleDto> result = await _service.GetVehiclesAsync(accessToken);

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
