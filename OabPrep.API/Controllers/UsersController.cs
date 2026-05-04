using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Performance.GetUserPerformance;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly GetUserPerformanceUseCase _getPerformanceUseCase;

    public UsersController(GetUserPerformanceUseCase getPerformanceUseCase) =>
        _getPerformanceUseCase = getPerformanceUseCase;

    [HttpGet("me/performance")]
    [ProducesResponseType(typeof(GetUserPerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] string period = "30d",
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _getPerformanceUseCase.ExecuteAsync(userId, period, cancellationToken);
        return Ok(result);
    }
}
