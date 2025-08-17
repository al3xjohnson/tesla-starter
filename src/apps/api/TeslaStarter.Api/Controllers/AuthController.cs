using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
using TeslaStarter.Api.Extensions;

namespace TeslaStarter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITeslaOAuthService teslaOAuthService,
    IMemoryCache cache,
    IMediator mediator,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly ITeslaOAuthService _teslaOAuthService = teslaOAuthService;
    private readonly IMemoryCache _cache = cache;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        GetUserByExternalIdQuery query = new() { ExternalId = descopeUserId };
        UserDto? user = await _mediator.Send(query);

        if (user == null)
        {
            // Create user if it doesn't exist
            CreateUserCommand createCommand = new()
            {
                ExternalId = descopeUserId,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
                DisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? ""
            };

            user = await _mediator.Send(createCommand);
        }

        return Ok(user);
    }

    /// <summary>
    /// Initialize Tesla OAuth flow
    /// </summary>
    [HttpGet("tesla/authorize")]
    [Authorize]
    public IActionResult InitiateTeslaAuth()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        // Generate state
        string state = TeslaOAuthService.GenerateState();

        // Store state for validation
        TeslaOAuthState oauthState = new()
        {
            State = state,
            DescopeUserId = descopeUserId,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set($"tesla_oauth_{state}", oauthState, TimeSpan.FromMinutes(10));

        string authUrl = _teslaOAuthService.GenerateAuthorizationUrl(state);

        return Ok(new { authUrl, state });
    }

    /// <summary>
    /// Handle Tesla OAuth callback
    /// </summary>
    [HttpPost("tesla/callback")]
    [Authorize]
    public async Task<IActionResult> HandleTeslaCallback([FromBody] TeslaCallbackRequest request)
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        // Validate state
        if (!_cache.TryGetValue($"tesla_oauth_{request.State}", out TeslaOAuthState? oauthState))
        {
            _logger.LogWarning("Invalid OAuth state for user {UserId}", descopeUserId);
            return BadRequest(new { error = "Invalid state" });
        }

        if (oauthState?.DescopeUserId != descopeUserId)
        {
            _logger.LogWarning("OAuth state user mismatch for user {UserId}", descopeUserId);
            return BadRequest(new { error = "State mismatch" });
        }

        if (oauthState.IsExpired)
        {
            _logger.LogWarning("OAuth state expired for user {UserId}", descopeUserId);
            return BadRequest(new { error = "State expired" });
        }

        // Remove state immediately to prevent reuse
        _cache.Remove($"tesla_oauth_{request.State}");

        // Exchange code for tokens
        TeslaTokenResponse? tokens = await _teslaOAuthService.ExchangeCodeForTokensAsync(request.Code!);
        if (tokens == null)
        {
            _logger.LogError("Failed to exchange code for tokens for user {UserId}", descopeUserId);
            return BadRequest(new { error = "Failed to exchange code for tokens" });
        }

        // Parse Tesla user info from ID token
        string teslaUserId = TeslaOAuthService.GetUserIdFromToken(tokens.IdToken);

        // Ensure we have required Tesla user info
        if (string.IsNullOrEmpty(teslaUserId))
        {
            _logger.LogError("No Tesla user ID found in ID token");
            return BadRequest(new { error = "Failed to get Tesla user information" });
        }

        // Update user with Tesla tokens
        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = descopeUserId,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            TeslaAccountId = teslaUserId
        };

        await _mediator.Send(command);

        // Sync vehicles from Tesla API
        try
        {
            SyncUserVehiclesCommand syncCommand = new() { ExternalId = descopeUserId };
            int syncedCount = await _mediator.Send(syncCommand);
            _logger.LogInformation("Synced {Count} vehicles for user {UserId}", syncedCount, descopeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync vehicles for user {UserId}", descopeUserId);
            // Don't fail the whole request if vehicle sync fails
        }

        return Ok(new { success = true, message = "Tesla account linked successfully" });
    }

    /// <summary>
    /// Refresh Tesla tokens
    /// </summary>
    [HttpPost("tesla/refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshTeslaTokens()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        GetUserByExternalIdQuery query = new() { ExternalId = descopeUserId };
        UserDto? user = await _mediator.Send(query);

        if (user == null)
            return NotFound(new { error = "User not found" });

        if (user.TeslaAccount == null)
            return BadRequest(new { error = "No Tesla account linked" });

        // Get the actual user entity to access tokens (not exposed in DTO)
        var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var userEntity = await userRepository.GetByExternalIdAsync(ExternalId.Create(descopeUserId));

        if (userEntity?.TeslaAccount?.RefreshToken == null)
            return BadRequest(new { error = "No refresh token available" });

        TeslaTokenResponse? tokens = await _teslaOAuthService.RefreshTokenAsync(userEntity.TeslaAccount.RefreshToken);
        if (tokens == null)
            return BadRequest(new { error = "Failed to refresh tokens" });

        // Update user with new tokens
        UpdateUserTeslaTokensCommand command = new()
        {
            ExternalId = descopeUserId,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt
        };

        await _mediator.Send(command);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Unlink Tesla account
    /// </summary>
    [HttpDelete("tesla/unlink")]
    [Authorize]
    public async Task<IActionResult> UnlinkTeslaAccount()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        GetUserByExternalIdQuery query = new() { ExternalId = descopeUserId };
        UserDto? user = await _mediator.Send(query);

        if (user == null)
            return NotFound(new { error = "User not found" });

        if (user.TeslaAccount != null)
        {
            // Get the actual user entity to access tokens (not exposed in DTO)
            var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var userEntity = await userRepository.GetByExternalIdAsync(ExternalId.Create(descopeUserId));

            if (userEntity?.TeslaAccount?.AccessToken != null)
            {
                // Revoke token with Tesla
                await _teslaOAuthService.RevokeTokenAsync(userEntity.TeslaAccount.AccessToken);
            }
        }

        // Remove tokens from user
        UnlinkTeslaAccountCommand command = new() { UserId = user.Id };
        await _mediator.Send(command);

        return Ok(new { success = true, message = "Tesla account unlinked" });
    }

    /// <summary>
    /// Get all users (requires authentication)
    /// </summary>
    [HttpGet("users")]
    [Authorize]
    public async Task<IActionResult> GetUsers()
    {
        GetUsersQuery query = new();
        List<UserDto> users = await _mediator.Send(query);
        return Ok(users);
    }

    /// <summary>
    /// Get current user's vehicles
    /// </summary>
    [HttpGet("vehicles")]
    [Authorize]
    public async Task<IActionResult> GetMyVehicles()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        GetUserByExternalIdQuery userQuery = new() { ExternalId = descopeUserId };
        UserDto? user = await _mediator.Send(userQuery);

        if (user == null)
            return NotFound(new { error = "User not found" });

        if (user.TeslaAccount == null)
            return Ok(new List<VehicleDto>());

        GetVehiclesByTeslaAccountQuery vehiclesQuery = new() { TeslaAccountId = user.TeslaAccount.TeslaAccountId };
        IReadOnlyList<VehicleDto> vehicles = await _mediator.Send(vehiclesQuery);

        return Ok(vehicles);
    }

    /// <summary>
    /// Sync vehicles from Tesla API
    /// </summary>
    [HttpPost("vehicles/sync")]
    [Authorize]
    public async Task<IActionResult> SyncVehicles()
    {
        string? descopeUserId = User.GetDescopeUserId();
        if (string.IsNullOrEmpty(descopeUserId))
            return Unauthorized();

        try
        {
            SyncUserVehiclesCommand syncCommand = new() { ExternalId = descopeUserId };
            int syncedCount = await _mediator.Send(syncCommand);

            return Ok(new { success = true, syncedCount, message = $"Synced {syncedCount} vehicles" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync vehicles for user {UserId}", descopeUserId);
            return BadRequest(new { error = "Failed to sync vehicles" });
        }
    }
}

public sealed record TeslaCallbackRequest
{
    public required string Code { get; init; }
    public required string State { get; init; }
}
