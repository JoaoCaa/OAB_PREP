using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using OabPrep.Application.UseCases.Sessions.SubmitAnswer;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly CreateSessionUseCase _createSessionUseCase;
    private readonly SubmitAnswerUseCase _submitAnswerUseCase;

    public SessionsController(
        CreateSessionUseCase createSessionUseCase,
        SubmitAnswerUseCase submitAnswerUseCase)
    {
        _createSessionUseCase = createSessionUseCase;
        _submitAnswerUseCase = submitAnswerUseCase;
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

    [HttpPost("{sessionId:int}/answers")]
    [ProducesResponseType(typeof(SubmitAnswerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAnswer(
        int sessionId,
        [FromBody] SubmitAnswerCommand command,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _submitAnswerUseCase.ExecuteAsync(
            command with { SessionId = sessionId }, userId, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
