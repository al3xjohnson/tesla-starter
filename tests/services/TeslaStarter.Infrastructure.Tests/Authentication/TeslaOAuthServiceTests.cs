using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Infrastructure.Authentication;
using TeslaStarter.Infrastructure.Configuration;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Authentication;

public sealed class TeslaOAuthServiceTests : IDisposable
{
    private readonly Mock<ILogger<TeslaOAuthService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly TeslaOAuthService _service;
    private readonly TeslaOptions _teslaOptions;

    public TeslaOAuthServiceTests()
    {
        _loggerMock = new Mock<ILogger<TeslaOAuthService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        _teslaOptions = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            RedirectUri = new Uri("https://test.com/callback")
        };

        IOptions<TeslaOptions> optionsMock = Options.Create(_teslaOptions);
        _service = new TeslaOAuthService(_httpClient, optionsMock, _loggerMock.Object);
    }

    [Fact]
    public void GenerateAuthorizationUrl_ReturnsCorrectUrl()
    {
        // Arrange
        string state = "test-state";

        // Act
        string url = _service.GenerateAuthorizationUrl(state);

        // Assert
        url.Should().StartWith("https://fleet-auth.prd.vn.cloud.tesla.com/oauth2/v3/authorize");
        url.Should().Contain($"client_id={Uri.EscapeDataString(_teslaOptions.ClientId)}");
        url.Should().Contain($"redirect_uri={Uri.EscapeDataString(_teslaOptions.RedirectUri.ToString())}");
        url.Should().Contain("response_type=code");
        url.Should().Contain("scope=openid%20vehicle_device_data%20offline_access");
        url.Should().Contain($"state={Uri.EscapeDataString(state)}");
    }

    [Fact]
    public void GenerateAuthorizationUrl_WithNullRedirectUri_HandlesCorrectly()
    {
        // Arrange
        TeslaOptions optionsWithNullUri = new()
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            RedirectUri = null!
        };
        IOptions<TeslaOptions> options = Options.Create(optionsWithNullUri);
        TeslaOAuthService service = new(_httpClient, options, _loggerMock.Object);
        string state = "test-state";

        // Act
        string url = service.GenerateAuthorizationUrl(state);

        // Assert
        url.Should().Contain("redirect_uri=");
    }

    [Fact]
    public async Task ExchangeCodeForTokensAsync_WithSuccessfulResponse_ReturnsTokens()
    {
        // Arrange
        string code = "test-code";
        Dictionary<string, object> responseData = new()
        {
            ["access_token"] = "test-access-token",
            ["refresh_token"] = "test-refresh-token",
            ["id_token"] = "test-id-token",
            ["token_type"] = "Bearer",
            ["expires_in"] = 3600
        };
        string responseJson = JsonSerializer.Serialize(responseData);

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
        TeslaTokenResponse? result = await _service.ExchangeCodeForTokensAsync(code);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.IdToken.Should().Be("test-id-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task ExchangeCodeForTokensAsync_WithFailedResponse_ReturnsNull()
    {
        // Arrange
        string code = "test-code";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Invalid request")
            });

        // Act
        TeslaTokenResponse? result = await _service.ExchangeCodeForTokensAsync(code);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to exchange Tesla code for tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExchangeCodeForTokensAsync_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        string code = "test-code";

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
        TeslaTokenResponse? result = await _service.ExchangeCodeForTokensAsync(code);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error exchanging Tesla code for tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExchangeCodeForTokensAsync_WithNullResponse_ReturnsNull()
    {
        // Arrange
        string code = "test-code";

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
                Content = new StringContent("null")
            });

        // Act
        TeslaTokenResponse? result = await _service.ExchangeCodeForTokensAsync(code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExchangeCodeForTokensAsync_WithException_ReturnsNull()
    {
        // Arrange
        string code = "test-code";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        TeslaTokenResponse? result = await _service.ExchangeCodeForTokensAsync(code);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error exchanging Tesla code for tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithSuccessfulResponse_ReturnsTokens()
    {
        // Arrange
        string refreshToken = "test-refresh-token";
        Dictionary<string, object> responseData = new()
        {
            ["access_token"] = "new-access-token",
            ["refresh_token"] = "new-refresh-token",
            ["id_token"] = "new-id-token",
            ["token_type"] = "Bearer",
            ["expires_in"] = 7200
        };
        string responseJson = JsonSerializer.Serialize(responseData);

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
        TeslaTokenResponse? result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(7200);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithFailedResponse_ReturnsNull()
    {
        // Arrange
        string refreshToken = "test-refresh-token";

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
                Content = new StringContent("Invalid token")
            });

        // Act
        TeslaTokenResponse? result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to refresh Tesla token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithException_ReturnsNull()
    {
        // Arrange
        string refreshToken = "test-refresh-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        TeslaTokenResponse? result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error refreshing Tesla token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task RevokeTokenAsync_WithSuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        string token = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        bool result = await _service.RevokeTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithFailedResponse_ReturnsFalse()
    {
        // Arrange
        string token = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        bool result = await _service.RevokeTokenAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithException_ReturnsFalse()
    {
        // Arrange
        string token = "test-token";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        bool result = await _service.RevokeTokenAsync(token);

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error revoking Tesla token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void GenerateState_ReturnsValidBase64UrlString()
    {
        // Act
        string state = TeslaOAuthService.GenerateState();

        // Assert
        state.Should().NotBeNullOrEmpty();
        state.Should().HaveLength(22); // 16 bytes encoded to base64 = 22 chars
        state.Should().NotContain("+");
        state.Should().NotContain("/");
        state.Should().NotContain("=");
        state.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void GenerateState_ProducesUniqueValues()
    {
        // Act
        string state1 = TeslaOAuthService.GenerateState();
        string state2 = TeslaOAuthService.GenerateState();
        string state3 = TeslaOAuthService.GenerateState();

        // Assert
        state1.Should().NotBe(state2);
        state2.Should().NotBe(state3);
        state1.Should().NotBe(state3);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        // This is a valid JWT token with sub claim "test-user-123"
        string validIdToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0LXVzZXItMTIzIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        // Act
        string userId = TeslaOAuthService.GetUserIdFromToken(validIdToken);

        // Assert
        userId.Should().Be("test-user-123");
    }

    [Fact]
    public void GetUserIdFromToken_WithTokenMissingSubClaim_ReturnsEmptyString()
    {
        // Arrange
        // JWT token without sub claim
        string tokenWithoutSub = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiSm9obiBEb2UiLCJpYXQiOjE1MTYyMzkwMjJ9.hqWGSaFpvbrXkOWc6lrnffhNWR19W_S1YKFBx2arWBk";

        // Act
        string userId = TeslaOAuthService.GetUserIdFromToken(tokenWithoutSub);

        // Assert
        userId.Should().BeEmpty();
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsEmptyString()
    {
        // Arrange
        string invalidToken = "not-a-valid-jwt-token";

        // Act
        string userId = TeslaOAuthService.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeEmpty();
    }

    [Fact]
    public void GetUserIdFromToken_WithEmptyToken_ReturnsEmptyString()
    {
        // Arrange
        string emptyToken = "";

        // Act
        string userId = TeslaOAuthService.GetUserIdFromToken(emptyToken);

        // Assert
        userId.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNullResponse_ReturnsNull()
    {
        // Arrange
        string refreshToken = "test-refresh-token";

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
                Content = new StringContent("null")
            });

        // Act
        TeslaTokenResponse? result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
