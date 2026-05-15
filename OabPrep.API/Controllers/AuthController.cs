using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OabPrep.API.Extensions;
using OabPrep.Application.UseCases.Auth.ConfirmEmail;
using OabPrep.Application.UseCases.Auth.ForgotPassword;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Application.UseCases.Auth.Logout;
using OabPrep.Application.UseCases.Auth.Refresh;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.Application.UseCases.Auth.ResetPassword;
using System.Security.Claims;

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
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly LogoutUseCase _logoutUseCase;

    public AuthController(
        RegisterUserUseCase registerUseCase,
        ConfirmEmailUseCase confirmEmailUseCase,
        LoginUseCase loginUseCase,
        ForgotPasswordUseCase forgotPasswordUseCase,
        ResetPasswordUseCase resetPasswordUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        LogoutUseCase logoutUseCase)
    {
        _registerUseCase = registerUseCase;
        _confirmEmailUseCase = confirmEmailUseCase;
        _loginUseCase = loginUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Public)]
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
    [EnableRateLimiting(RateLimitPolicies.Public)]
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
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
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
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
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
    [EnableRateLimiting(RateLimitPolicies.Public)]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _resetPasswordUseCase.ExecuteAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicies.Public)]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _refreshTokenUseCase.ExecuteAsync(command, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    [EnableRateLimiting(RateLimitPolicies.Standard)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _logoutUseCase.ExecuteAsync(userId, cancellationToken);
        return NoContent();
    }
}
