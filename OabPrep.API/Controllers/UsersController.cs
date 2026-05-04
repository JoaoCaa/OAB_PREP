using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.Performance.GetAreaPerformance;
using OabPrep.Application.UseCases.Performance.GetUserPerformance;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly GetUserPerformanceUseCase _getPerformanceUseCase;
    private readonly GetAreaPerformanceUseCase _getAreaPerformanceUseCase;

    public UsersController(
        GetUserPerformanceUseCase getPerformanceUseCase,
        GetAreaPerformanceUseCase getAreaPerformanceUseCase)
    {
        _getPerformanceUseCase = getPerformanceUseCase;
        _getAreaPerformanceUseCase = getAreaPerformanceUseCase;
    }

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

    [HttpGet("me/performance/areas/{areaId:int}")]
    [ProducesResponseType(typeof(GetAreaPerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAreaPerformance(int areaId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _getAreaPerformanceUseCase.ExecuteAsync(userId, areaId, cancellationToken);
        return Ok(result);
    }
}
