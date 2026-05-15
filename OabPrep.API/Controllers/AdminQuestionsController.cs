using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OabPrep.API.Extensions;
using OabPrep.Application.Common.Models;
using OabPrep.Application.UseCases.Questions;
using OabPrep.Application.UseCases.Questions.Create;
using OabPrep.Application.UseCases.Questions.Deactivate;
using OabPrep.Application.UseCases.Questions.GetById;
using OabPrep.Application.UseCases.Questions.GetList;
using OabPrep.Application.UseCases.Questions.Update;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/admin/questions")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting(RateLimitPolicies.Standard)]
public sealed class AdminQuestionsController : ControllerBase
{
    private readonly CreateQuestionUseCase _createUseCase;
    private readonly UpdateQuestionUseCase _updateUseCase;
    private readonly DeactivateQuestionUseCase _deactivateUseCase;
    private readonly GetQuestionsUseCase _getListUseCase;
    private readonly GetQuestionByIdUseCase _getByIdUseCase;

    public AdminQuestionsController(
        CreateQuestionUseCase createUseCase,
        UpdateQuestionUseCase updateUseCase,
        DeactivateQuestionUseCase deactivateUseCase,
        GetQuestionsUseCase getListUseCase,
        GetQuestionByIdUseCase getByIdUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _deactivateUseCase = deactivateUseCase;
        _getListUseCase = getListUseCase;
        _getByIdUseCase = getByIdUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<QuestionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int? areaId,
        [FromQuery] int? year,
        [FromQuery] int? difficulty,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery(areaId, year, difficulty, search, page, pageSize);
        var result = await _getListUseCase.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuestionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuestionDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var adminId = GetAdminUserId();
        var result = await _createUseCase.ExecuteAsync(command, adminId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(QuestionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var adminId = GetAdminUserId();
        var result = await _updateUseCase.ExecuteAsync(command with { Id = id }, adminId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var adminId = GetAdminUserId();
        await _deactivateUseCase.ExecuteAsync(id, adminId, cancellationToken);
        return NoContent();
    }

    private Guid GetAdminUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
