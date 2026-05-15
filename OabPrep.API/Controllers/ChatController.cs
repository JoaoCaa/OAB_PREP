using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OabPrep.API.Extensions;
using OabPrep.Application.UseCases.Chat.SendMessage;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/chat")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.Chat)]
public sealed class ChatController : ControllerBase
{
    private readonly SendChatMessageUseCase _sendMessageUseCase;

    public ChatController(SendChatMessageUseCase sendMessageUseCase)
    {
        _sendMessageUseCase = sendMessageUseCase;
    }

    [HttpPost("messages")]
    [ProducesResponseType(typeof(SendChatMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sendMessageUseCase.ExecuteAsync(command, GetUserId(), cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
