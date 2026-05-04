using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Performance.GetAreaPerformance;
using OabPrep.Application.UseCases.Performance.GetUserPerformance;
using OabPrep.Application.UseCases.Profile.ChangePassword;
using OabPrep.Application.UseCases.Profile.DeleteAccount;
using OabPrep.Application.UseCases.Profile.GetProfile;
using OabPrep.Application.UseCases.Profile.RequestDataExport;
using OabPrep.Application.UseCases.Profile.UpdateProfile;
using OabPrep.Application.UseCases.Profile.UploadAvatar;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly GetUserPerformanceUseCase _getPerformanceUseCase;
    private readonly GetAreaPerformanceUseCase _getAreaPerformanceUseCase;
    private readonly GetProfileUseCase _getProfileUseCase;
    private readonly UpdateProfileUseCase _updateProfileUseCase;
    private readonly ChangePasswordUseCase _changePasswordUseCase;
    private readonly UploadAvatarUseCase _uploadAvatarUseCase;
    private readonly DeleteAccountUseCase _deleteAccountUseCase;
    private readonly RequestDataExportUseCase _requestDataExportUseCase;

    public UsersController(
        GetUserPerformanceUseCase getPerformanceUseCase,
        GetAreaPerformanceUseCase getAreaPerformanceUseCase,
        GetProfileUseCase getProfileUseCase,
        UpdateProfileUseCase updateProfileUseCase,
        ChangePasswordUseCase changePasswordUseCase,
        UploadAvatarUseCase uploadAvatarUseCase,
        DeleteAccountUseCase deleteAccountUseCase,
        RequestDataExportUseCase requestDataExportUseCase)
    {
        _getPerformanceUseCase = getPerformanceUseCase;
        _getAreaPerformanceUseCase = getAreaPerformanceUseCase;
        _getProfileUseCase = getProfileUseCase;
        _updateProfileUseCase = updateProfileUseCase;
        _changePasswordUseCase = changePasswordUseCase;
        _uploadAvatarUseCase = uploadAvatarUseCase;
        _deleteAccountUseCase = deleteAccountUseCase;
        _requestDataExportUseCase = requestDataExportUseCase;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _getProfileUseCase.ExecuteAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        await _updateProfileUseCase.ExecuteAsync(GetUserId(), command, cancellationToken);
        return NoContent();
    }

    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _changePasswordUseCase.ExecuteAsync(GetUserId(), command, cancellationToken);
        return NoContent();
    }

    [HttpPost("me/avatar")]
    [ProducesResponseType(typeof(UploadAvatarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    [RequestSizeLimit(2 * 1024 * 1024 + 4096)]
    public async Task<IActionResult> UploadAvatar(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await _uploadAvatarUseCase.ExecuteAsync(
            GetUserId(), stream, file.ContentType, file.Length, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        await _deleteAccountUseCase.ExecuteAsync(GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("me/data-export")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestDataExport(CancellationToken cancellationToken)
    {
        await _requestDataExportUseCase.ExecuteAsync(GetUserId(), cancellationToken);
        return Accepted();
    }

    [HttpGet("me/performance")]
    [ProducesResponseType(typeof(GetUserPerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] string period = "30d",
        CancellationToken cancellationToken = default)
    {
        var result = await _getPerformanceUseCase.ExecuteAsync(GetUserId(), period, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me/performance/areas/{areaId:int}")]
    [ProducesResponseType(typeof(GetAreaPerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAreaPerformance(int areaId, CancellationToken cancellationToken)
    {
        var result = await _getAreaPerformanceUseCase.ExecuteAsync(GetUserId(), areaId, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
