using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly CreateSessionUseCase _createSessionUseCase;

    public SessionsController(CreateSessionUseCase createSessionUseCase)
    {
        _createSessionUseCase = createSessionUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSessionCommand command,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _createSessionUseCase.ExecuteAsync(command, userId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
