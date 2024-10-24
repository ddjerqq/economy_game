using Application.Economy.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Middleware;

namespace WebApi.Controllers.V1;

/// <summary>
/// Controller for economy commands
/// </summary>
[Authorize]
public sealed class EconomyController(ILogger<EconomyController> logger, IMediator mediator) : ApiController(logger)
{
    /// <summary>
    /// Sends money to a user
    /// </summary>
    [HttpPost("send")]
    [RequireIdempotency]
    public async Task<ActionResult<bool>> SendToUser(UserTransactionCommand command, CancellationToken ct)
    {
        var success = await mediator.Send(command, ct);
        if (success)
            return Ok(true);

        return BadRequest("user not found, or insufficient funds");
    }
}