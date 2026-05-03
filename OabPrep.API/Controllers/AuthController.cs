using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Auth.Register;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUseCase;

    public AuthController(RegisterUserUseCase registerUseCase) =>
        _registerUseCase = registerUseCase;

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
}
