using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TeslaStarter.Application.Users.Commands.CreateUser;
using TeslaStarter.Application.Users.Commands.LinkTeslaAccount;
using TeslaStarter.Application.Users.Commands.RecordLogin;
using TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;
using TeslaStarter.Application.Users.Commands.UpdateProfile;
using TeslaStarter.Application.Users.DTOs;
using TeslaStarter.Application.Users.Queries.GetUser;
using TeslaStarter.Application.Users.Queries.GetUserByExternalId;

namespace TeslaStarter.Api.Controllers;

[ApiVersion("1.0")]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        GetUserQuery query = new() { UserId = id };
        UserDto? result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a user by external ID
    /// </summary>
    /// <param name="externalId">The external ID from the authentication provider</param>
    /// <returns>The user details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("external/{externalId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserByExternalId(string externalId, CancellationToken cancellationToken)
    {
        GetUserByExternalIdQuery query = new() { ExternalId = externalId };
        UserDto? result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="command">The user creation details</param>
    /// <returns>The created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="409">If a user with the same external ID already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        UserDto result = await Mediator.Send(command, cancellationToken);
        return Created(result, $"api/v1/users/{result.Id}");
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">The profile update details</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the user is not found</response>
    [HttpPut("{id:guid}/profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        UpdateProfileCommand command = new()
        {
            UserId = id,
            Email = request.Email ?? string.Empty,
            DisplayName = request.DisplayName
        };

        UserDto result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Link a Tesla account to the user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">The Tesla account details</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the user is not found</response>
    [HttpPost("{id:guid}/tesla-account")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> LinkTeslaAccount(Guid id, [FromBody] LinkTeslaAccountRequest request, CancellationToken cancellationToken)
    {
        LinkTeslaAccountCommand command = new()
        {
            UserId = id,
            TeslaAccountId = request.TeslaAccountId
        };

        UserDto result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Unlink the Tesla account from the user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="404">If the user is not found</response>
    [HttpDelete("{id:guid}/tesla-account")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UnlinkTeslaAccount(Guid id, CancellationToken cancellationToken)
    {
        UnlinkTeslaAccountCommand command = new() { UserId = id };
        UserDto result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Record a user login
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="404">If the user is not found</response>
    [HttpPost("{id:guid}/login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> RecordLogin(Guid id, CancellationToken cancellationToken)
    {
        RecordLoginCommand command = new() { UserId = id };
        UserDto result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public sealed record UpdateProfileRequest
{
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
}

public sealed record LinkTeslaAccountRequest
{
    public required string TeslaAccountId { get; init; }
}
