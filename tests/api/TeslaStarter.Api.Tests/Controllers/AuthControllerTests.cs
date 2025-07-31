using System.Security.Claims;
using System.Text.Json;
using Common.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using TeslaStarter.Api.Controllers;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Application.Users.Commands.CreateUser;
using TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;
using TeslaStarter.Application.Users.Commands.UpdateUserTeslaTokens;
using TeslaStarter.Application.Users.DTOs;
using TeslaStarter.Application.Users.Queries.GetUserByExternalId;
using TeslaStarter.Application.Users.Queries.GetUsers;
using TeslaStarter.Application.Vehicles.Commands.SyncUserVehicles;
using TeslaStarter.Application.Vehicles.DTOs;
using TeslaStarter.Application.Vehicles.Queries.GetVehiclesByTeslaAccount;
using TeslaStarter.Domain.Users;
using TeslaStarter.Infrastructure.Authentication;
using Xunit;

namespace TeslaStarter.Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    private readonly Mock<ITeslaOAuthService> _teslaOAuthServiceMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;
    private readonly Mock<ICacheEntry> _cacheEntryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public AuthControllerTests()
    {
        _teslaOAuthServiceMock = new Mock<ITeslaOAuthService>();
        _cacheMock = new Mock<IMemoryCache>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _cacheEntryMock = new Mock<ICacheEntry>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository)))
            .Returns(_userRepositoryMock.Object);

        _controller = new AuthController(
            _teslaOAuthServiceMock.Object,
            _cacheMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    private void SetupUser(string userId, string email = "test@example.com", string name = "Test User")
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name)
        ];

        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal,
                RequestServices = _serviceProviderMock.Object
            }
        };
    }

    private static JsonElement GetJsonElement(object value)
    {
        string json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    private void SetupUserEntity(string externalId, string? refreshToken = null, string? accessToken = null)
    {
        User? userEntity = null;
        if (refreshToken != null || accessToken != null)
        {
            userEntity = User.Create(ExternalId.Create(externalId), Email.Create("test@example.com"), "Test User");

            // First link the Tesla account
            userEntity.LinkTeslaAccount("tesla123");

            // Then update the tokens using the User's method
            if (refreshToken != null || accessToken != null)
            {
                userEntity.UpdateTeslaTokens(accessToken ?? "access", refreshToken ?? "refresh", DateTime.UtcNow.AddHours(1));
            }
        }

        _userRepositoryMock.Setup(x => x.GetByExternalIdAsync(It.Is<ExternalId>(e => e.Value == externalId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);
    }

    [Fact]
    public async Task GetCurrentUser_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserExists_ReturnsOkWithUser()
    {
        // Arrange
        SetupUser("user123");
        UserDto existingUser = new() { Id = Guid.NewGuid(), ExternalId = "user123" };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(existingUser);

        // Act
        IActionResult result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(existingUser);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserDoesNotExist_CreatesUserAndReturnsOk()
    {
        // Arrange
        SetupUser("user123", "new@example.com", "New User");
        UserDto newUser = new() { Id = Guid.NewGuid(), ExternalId = "user123" };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync((UserDto?)null);

        _mediatorMock.Setup(x => x.Send(It.IsAny<CreateUserCommand>(), default))
            .ReturnsAsync(newUser);

        // Act
        IActionResult result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(newUser);

        _mediatorMock.Verify(x => x.Send(
            It.Is<CreateUserCommand>(cmd =>
                cmd.ExternalId == "user123" &&
                cmd.Email == "new@example.com" &&
                cmd.DisplayName == "New User"),
            default), Times.Once);
    }

    [Fact]
    public void InitiateTeslaAuth_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = _controller.InitiateTeslaAuth();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void InitiateTeslaAuth_WhenValidUser_ReturnsOkWithAuthUrlAndState()
    {
        // Arrange
        SetupUser("user123");
        string expectedAuthUrl = "https://auth.tesla.com/oauth2/v3/authorize?...";

        _teslaOAuthServiceMock.Setup(x => x.GenerateAuthorizationUrl(It.IsAny<string>()))
            .Returns(expectedAuthUrl);

        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(_cacheEntryMock.Object);

        // Act
        IActionResult result = _controller.InitiateTeslaAuth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("authUrl").GetString().Should().Be(expectedAuthUrl);
        json.GetProperty("state").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenInvalidState_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "invalid_state" };

        object? outValue = null;
        _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("Invalid state");
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenStateMismatch_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "differentUser", CreatedAt = DateTime.UtcNow };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("State mismatch");
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenStateExpired_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "user123", CreatedAt = DateTime.UtcNow.AddMinutes(-11) };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("State expired");
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenTokenExchangeFails_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "user123", CreatedAt = DateTime.UtcNow };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        _teslaOAuthServiceMock.Setup(x => x.ExchangeCodeForTokensAsync(request.Code))
            .ReturnsAsync((TeslaTokenResponse?)null);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("Failed to exchange code for tokens");
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenNoTeslaUserIdInToken_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "user123", CreatedAt = DateTime.UtcNow };
        TeslaTokenResponse tokens = new()
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            IdToken = "id_token",
            ExpiresIn = 3600
        };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        _teslaOAuthServiceMock.Setup(x => x.ExchangeCodeForTokensAsync(request.Code))
            .ReturnsAsync(tokens);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("Failed to get Tesla user information");
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenSuccessful_ReturnsOkAndSyncsVehicles()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "user123", CreatedAt = DateTime.UtcNow };
        TeslaTokenResponse tokens = new()
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            IdToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXNsYTEyMyIsImlhdCI6MTUxNjIzOTAyMn0.test",
            ExpiresIn = 3600
        };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        _teslaOAuthServiceMock.Setup(x => x.ExchangeCodeForTokensAsync(request.Code))
            .ReturnsAsync(tokens);

        _mediatorMock.Setup(x => x.Send(It.IsAny<SyncUserVehiclesCommand>(), default))
            .ReturnsAsync(3);

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("message").GetString().Should().Be("Tesla account linked successfully");

        _cacheMock.Verify(x => x.Remove($"tesla_oauth_{request.State}"), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateUserTeslaTokensCommand>(), default), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<SyncUserVehiclesCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleTeslaCallback_WhenVehicleSyncFails_StillReturnsOk()
    {
        // Arrange
        SetupUser("user123");
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };
        TeslaOAuthState oauthState = new() { State = "state123", DescopeUserId = "user123", CreatedAt = DateTime.UtcNow };
        TeslaTokenResponse tokens = new()
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            IdToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXNsYTEyMyIsImlhdCI6MTUxNjIzOTAyMn0.test",
            ExpiresIn = 3600
        };

        object? outValue = oauthState;
        _cacheMock.Setup(x => x.TryGetValue($"tesla_oauth_{request.State}", out outValue))
            .Returns(true);

        _teslaOAuthServiceMock.Setup(x => x.ExchangeCodeForTokensAsync(request.Code))
            .ReturnsAsync(tokens);

        _mediatorMock.Setup(x => x.Send(It.IsAny<SyncUserVehiclesCommand>(), default))
            .ThrowsAsync(new Exception("Sync failed"));

        // Act
        IActionResult result = await _controller.HandleTeslaCallback(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("message").GetString().Should().Be("Tesla account linked successfully");
    }

    [Fact]
    public async Task RefreshTeslaTokens_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = await _controller.RefreshTeslaTokens();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task RefreshTeslaTokens_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupUser("user123");

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync((UserDto?)null);

        // Act
        IActionResult result = await _controller.RefreshTeslaTokens();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        NotFoundObjectResult notFound = (NotFoundObjectResult)result;
        JsonElement json = GetJsonElement(notFound.Value!);
        json.GetProperty("error").GetString().Should().Be("User not found");
    }

    [Fact]
    public async Task RefreshTeslaTokens_WhenNoTeslaAccountLinked_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = null };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        // Act
        IActionResult result = await _controller.RefreshTeslaTokens();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("No Tesla account linked");
    }

    [Fact]
    public async Task RefreshTeslaTokens_WhenRefreshFails_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");
        SetupUserEntity("user123", refreshToken: "refresh_token");
        TeslaAccountDto teslaAccount = new() { TeslaAccountId = "tesla123" };
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = teslaAccount };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        _teslaOAuthServiceMock.Setup(x => x.RefreshTokenAsync("refresh_token"))
            .ReturnsAsync((TeslaTokenResponse?)null);

        // Act
        IActionResult result = await _controller.RefreshTeslaTokens();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("Failed to refresh tokens");
    }

    [Fact]
    public async Task RefreshTeslaTokens_WhenSuccessful_ReturnsOk()
    {
        // Arrange
        SetupUser("user123");
        SetupUserEntity("user123", refreshToken: "refresh_token");
        TeslaAccountDto teslaAccount = new() { TeslaAccountId = "tesla123" };
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = teslaAccount };
        TeslaTokenResponse newTokens = new()
        {
            AccessToken = "new_access",
            RefreshToken = "new_refresh",
            IdToken = "id_token",
            ExpiresIn = 3600
        };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        _teslaOAuthServiceMock.Setup(x => x.RefreshTokenAsync("refresh_token"))
            .ReturnsAsync(newTokens);

        // Act
        IActionResult result = await _controller.RefreshTeslaTokens();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateUserTeslaTokensCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task UnlinkTeslaAccount_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = await _controller.UnlinkTeslaAccount();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task UnlinkTeslaAccount_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupUser("user123");

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync((UserDto?)null);

        // Act
        IActionResult result = await _controller.UnlinkTeslaAccount();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UnlinkTeslaAccount_WhenUserHasAccessToken_RevokesAndUnlinks()
    {
        // Arrange
        SetupUser("user123");
        SetupUserEntity("user123", accessToken: "access_token");
        TeslaAccountDto teslaAccount = new() { TeslaAccountId = "tesla123" };
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = teslaAccount };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        // Act
        IActionResult result = await _controller.UnlinkTeslaAccount();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("message").GetString().Should().Be("Tesla account unlinked");

        _teslaOAuthServiceMock.Verify(x => x.RevokeTokenAsync("access_token"), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.Is<UnlinkTeslaAccountCommand>(cmd => cmd.UserId == user.Id), default), Times.Once);
    }

    [Fact]
    public async Task UnlinkTeslaAccount_WhenUserHasNoAccessToken_JustUnlinks()
    {
        // Arrange
        SetupUser("user123");
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = null };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        // Act
        IActionResult result = await _controller.UnlinkTeslaAccount();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("message").GetString().Should().Be("Tesla account unlinked");

        _teslaOAuthServiceMock.Verify(x => x.RevokeTokenAsync(It.IsAny<string>()), Times.Never);
        _mediatorMock.Verify(x => x.Send(It.Is<UnlinkTeslaAccountCommand>(cmd => cmd.UserId == user.Id), default), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUserList()
    {
        // Arrange
        SetupUser("user123");
        List<UserDto> users =
        [
            new() { Id = Guid.NewGuid(), ExternalId = "user1" },
            new() { Id = Guid.NewGuid(), ExternalId = "user2" }
        ];

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUsersQuery>(), default))
            .ReturnsAsync(users);

        // Act
        IActionResult result = await _controller.GetUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(users);
    }

    [Fact]
    public async Task GetMyVehicles_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = await _controller.GetMyVehicles();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetMyVehicles_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupUser("user123");

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync((UserDto?)null);

        // Act
        IActionResult result = await _controller.GetMyVehicles();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMyVehicles_WhenNoTeslaAccount_ReturnsEmptyList()
    {
        // Arrange
        SetupUser("user123");
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = null };

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        // Act
        IActionResult result = await _controller.GetMyVehicles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(new List<VehicleDto>());
    }

    [Fact]
    public async Task GetMyVehicles_WhenHasTeslaAccount_ReturnsVehicles()
    {
        // Arrange
        SetupUser("user123");
        TeslaAccountDto teslaAccount = new() { TeslaAccountId = "tesla123" };
        UserDto user = new() { Id = Guid.NewGuid(), ExternalId = "user123", TeslaAccount = teslaAccount };
        List<VehicleDto> vehicles =
        [
            new() { Id = Guid.NewGuid(), VehicleIdentifier = "vehicle1" },
            new() { Id = Guid.NewGuid(), VehicleIdentifier = "vehicle2" }
        ];

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByExternalIdQuery>(), default))
            .ReturnsAsync(user);

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetVehiclesByTeslaAccountQuery>(), default))
            .ReturnsAsync(vehicles);

        // Act
        IActionResult result = await _controller.GetMyVehicles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(vehicles);
    }

    [Fact]
    public async Task SyncVehicles_WhenNoUserIdInClaims_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(),
                RequestServices = _serviceProviderMock.Object
            }
        };

        // Act
        IActionResult result = await _controller.SyncVehicles();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task SyncVehicles_WhenSuccessful_ReturnsOkWithCount()
    {
        // Arrange
        SetupUser("user123");

        _mediatorMock.Setup(x => x.Send(It.IsAny<SyncUserVehiclesCommand>(), default))
            .ReturnsAsync(5);

        // Act
        IActionResult result = await _controller.SyncVehicles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        OkObjectResult okResult = (OkObjectResult)result;
        JsonElement json = GetJsonElement(okResult.Value!);
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("syncedCount").GetInt32().Should().Be(5);
        json.GetProperty("message").GetString().Should().Be("Synced 5 vehicles");
    }

    [Fact]
    public async Task SyncVehicles_WhenFails_ReturnsBadRequest()
    {
        // Arrange
        SetupUser("user123");

        _mediatorMock.Setup(x => x.Send(It.IsAny<SyncUserVehiclesCommand>(), default))
            .ThrowsAsync(new Exception("Sync error"));

        // Act
        IActionResult result = await _controller.SyncVehicles();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
        JsonElement json = GetJsonElement(badRequest.Value!);
        json.GetProperty("error").GetString().Should().Be("Failed to sync vehicles");
    }

    [Fact]
    public void TeslaCallbackRequest_RequiredProperties_AreSet()
    {
        // Arrange & Act
        TeslaCallbackRequest request = new() { Code = "code123", State = "state123" };

        // Assert
        request.Code.Should().Be("code123");
        request.State.Should().Be("state123");
    }
}
