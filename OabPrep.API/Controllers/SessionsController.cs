using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using OabPrep.Application.UseCases.Sessions.FinishSession;
using OabPrep.Application.UseCases.Sessions.GetSession;
using OabPrep.Application.UseCases.Sessions.SubmitAnswer;
using OabPrep.Application.UseCases.Sessions.ToggleReviewMark;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly CreateSessionUseCase _createSessionUseCase;
    private readonly SubmitAnswerUseCase _submitAnswerUseCase;
    private readonly FinishSessionUseCase _finishSessionUseCase;
    private readonly ToggleReviewMarkUseCase _toggleReviewMarkUseCase;
    private readonly GetSessionUseCase _getSessionUseCase;

    public SessionsController(
        CreateSessionUseCase createSessionUseCase,
        SubmitAnswerUseCase submitAnswerUseCase,
        FinishSessionUseCase finishSessionUseCase,
        ToggleReviewMarkUseCase toggleReviewMarkUseCase,
        GetSessionUseCase getSessionUseCase)
    {
        _createSessionUseCase = createSessionUseCase;
        _submitAnswerUseCase = submitAnswerUseCase;
        _finishSessionUseCase = finishSessionUseCase;
        _toggleReviewMarkUseCase = toggleReviewMarkUseCase;
        _getSessionUseCase = getSessionUseCase;
    }

    [HttpGet("{sessionId:int}")]
    [ProducesResponseType(typeof(GetSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getSessionUseCase.ExecuteAsync(sessionId, userId, cancellationToken);
        return Ok(result);
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

    [HttpPost("{sessionId:int}/finish")]
    [ProducesResponseType(typeof(FinishSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finish(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _finishSessionUseCase.ExecuteAsync(sessionId, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{sessionId:int}/answers/{questionId:int}/review")]
    [ProducesResponseType(typeof(ToggleReviewMarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleReview(
        int sessionId,
        int questionId,
        [FromBody] ToggleReviewMarkCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _toggleReviewMarkUseCase.ExecuteAsync(sessionId, questionId, command, userId, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
