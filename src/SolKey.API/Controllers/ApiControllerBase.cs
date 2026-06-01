using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;

namespace SolKey.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<ActionResult<ResponseEnvelope<T>>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Ok(ResponseEnvelope<T>.Success(result));
        }
        catch (InvalidOperationException ex)
        {
            return MapInvalidOperation<T>(ex.Message);
        }
    }

    protected async Task<ActionResult<ResponseEnvelope<object>>> ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Ok(ResponseEnvelope<object>.Success(new { }));
        }
        catch (InvalidOperationException ex)
        {
            return MapInvalidOperationObject(ex.Message);
        }
    }

    private ActionResult<ResponseEnvelope<T>> MapInvalidOperation<T>(string? message)
    {
        var safeMessage = message ?? "An error occurred.";
        var lower = safeMessage.ToLowerInvariant();
        if (lower.Contains("not found"))
        {
            return NotFound(ResponseEnvelope<T>.Failure(safeMessage));
        }

        if (lower.Contains("access denied"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ResponseEnvelope<T>.Failure(safeMessage));
        }

        if (lower.Contains("unauthorized"))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ResponseEnvelope<T>.Failure(safeMessage));
        }

        return BadRequest(ResponseEnvelope<T>.Failure(safeMessage));
    }

    private ActionResult<ResponseEnvelope<object>> MapInvalidOperationObject(string? message)
    {
        return MapInvalidOperation<object>(message);
    }
}
