using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TeslaStarter.Application.Vehicles.Commands.LinkVehicle;
using TeslaStarter.Application.Vehicles.Commands.UnlinkVehicle;
using TeslaStarter.Application.Vehicles.Commands.UpdateVehicle;
using TeslaStarter.Application.Vehicles.DTOs;
using TeslaStarter.Application.Vehicles.Queries.GetVehicle;
using TeslaStarter.Application.Vehicles.Queries.GetVehiclesByTeslaAccount;

namespace TeslaStarter.Api.Controllers;

[ApiVersion("1.0")]
public class VehiclesController : ApiControllerBase
{
    /// <summary>
    /// Get a vehicle by ID
    /// </summary>
    /// <param name="id">The vehicle ID</param>
    /// <returns>The vehicle details</returns>
    /// <response code="200">Returns the vehicle</response>
    /// <response code="404">If the vehicle is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDto>> GetVehicle(Guid id, CancellationToken cancellationToken)
    {
        GetVehicleQuery query = new() { VehicleId = id };
        VehicleDto? result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all vehicles for a Tesla account
    /// </summary>
    /// <param name="teslaAccountId">The Tesla account ID</param>
    /// <returns>List of vehicles</returns>
    /// <response code="200">Returns the list of vehicles</response>
    [HttpGet("tesla-account/{teslaAccountId}")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleDto>>> GetVehiclesByTeslaAccount(string teslaAccountId, CancellationToken cancellationToken)
    {
        GetVehiclesByTeslaAccountQuery query = new() { TeslaAccountId = teslaAccountId };
        IReadOnlyList<VehicleDto>? result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Link a new vehicle
    /// </summary>
    /// <param name="command">The vehicle details</param>
    /// <returns>The linked vehicle</returns>
    /// <response code="201">Returns the newly linked vehicle</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleDto>> LinkVehicle([FromBody] LinkVehicleCommand command, CancellationToken cancellationToken)
    {
        VehicleDto result = await Mediator.Send(command, cancellationToken);
        return Created(result, $"api/v1/vehicles/{result.Id}");
    }

    /// <summary>
    /// Update vehicle details
    /// </summary>
    /// <param name="id">The vehicle ID</param>
    /// <param name="request">The update details</param>
    /// <returns>The updated vehicle</returns>
    /// <response code="200">Returns the updated vehicle</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the vehicle is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDto>> UpdateVehicle(Guid id, [FromBody] UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        UpdateVehicleCommand command = new()
        {
            VehicleId = id,
            DisplayName = request.DisplayName
        };

        VehicleDto result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Unlink (deactivate) a vehicle
    /// </summary>
    /// <param name="id">The vehicle ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Vehicle successfully unlinked</response>
    /// <response code="404">If the vehicle is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkVehicle(Guid id, CancellationToken cancellationToken)
    {
        UnlinkVehicleCommand command = new() { VehicleId = id };
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateVehicleRequest
{
    public string? DisplayName { get; init; }
}
