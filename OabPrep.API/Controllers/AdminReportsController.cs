using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OabPrep.API.Extensions;
using OabPrep.Application.UseCases.Admin.Reports.GetQuestionStats;
using OabPrep.Application.UseCases.Admin.Reports.GetSystemSummary;
using OabPrep.Domain.Constants;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = UserRoles.Admin)]
[EnableRateLimiting(RateLimitPolicies.Standard)]
public sealed class AdminReportsController : ControllerBase
{
    private readonly GetSystemSummaryUseCase _summaryUseCase;
    private readonly GetQuestionStatsUseCase _questionStatsUseCase;

    public AdminReportsController(
        GetSystemSummaryUseCase summaryUseCase,
        GetQuestionStatsUseCase questionStatsUseCase)
    {
        _summaryUseCase = summaryUseCase;
        _questionStatsUseCase = questionStatsUseCase;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SystemSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _summaryUseCase.ExecuteAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("questions")]
    [ProducesResponseType(typeof(QuestionStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetQuestionStats(
        [FromQuery] int? areaId,
        CancellationToken cancellationToken)
    {
        var result = await _questionStatsUseCase.ExecuteAsync(areaId, cancellationToken);
        return Ok(result);
    }
}
