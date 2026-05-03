using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Auth.ConfirmEmail;
using OabPrep.Application.UseCases.Auth.ForgotPassword;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.Application.UseCases.Auth.ResetPassword;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUseCase;
    private readonly ConfirmEmailUseCase _confirmEmailUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;

    public AuthController(
        RegisterUserUseCase registerUseCase,
        ConfirmEmailUseCase confirmEmailUseCase,
        LoginUseCase loginUseCase,
        ForgotPasswordUseCase forgotPasswordUseCase,
        ResetPasswordUseCase resetPasswordUseCase)
    {
        _registerUseCase = registerUseCase;
        _confirmEmailUseCase = confirmEmailUseCase;
        _loginUseCase = loginUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _loginUseCase.ExecuteAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _forgotPasswordUseCase.ExecuteAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _resetPasswordUseCase.ExecuteAsync(command, cancellationToken);
        return Ok(response);
    }
}
