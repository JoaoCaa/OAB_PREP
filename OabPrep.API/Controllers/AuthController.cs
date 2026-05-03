using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Auth.ConfirmEmail;
using OabPrep.Application.UseCases.Auth.Register;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUseCase;
    private readonly ConfirmEmailUseCase _confirmEmailUseCase;

    public AuthController(
        RegisterUserUseCase registerUseCase,
        ConfirmEmailUseCase confirmEmailUseCase)
    {
        _registerUseCase = registerUseCase;
        _confirmEmailUseCase = confirmEmailUseCase;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _registerUseCase.ExecuteAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var response = await _confirmEmailUseCase.ExecuteAsync(token, cancellationToken);
        return Redirect(response.RedirectUrl);
    }
}
