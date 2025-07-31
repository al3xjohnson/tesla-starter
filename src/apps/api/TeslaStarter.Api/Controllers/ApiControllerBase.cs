using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TeslaStarter.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = null!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<T> HandleResult<T>(T? result)
    {
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    protected ActionResult HandleResult(bool result)
    {
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    protected ActionResult<T> Created<T>(T result, string? location = null)
    {
        if (location != null)
        {
            return Created(location, result);
        }

        return CreatedAtAction(null, result);
    }
}
