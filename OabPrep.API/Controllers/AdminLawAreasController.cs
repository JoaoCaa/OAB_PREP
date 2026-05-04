using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.LawAreas;
using OabPrep.Application.UseCases.LawAreas.Create;
using OabPrep.Application.UseCases.LawAreas.Deactivate;
using OabPrep.Application.UseCases.LawAreas.Update;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/admin/law-areas")]
[Authorize(Roles = "Admin")]
public sealed class AdminLawAreasController : ControllerBase
{
    private readonly CreateLawAreaUseCase _createUseCase;
    private readonly UpdateLawAreaUseCase _updateUseCase;
    private readonly DeactivateLawAreaUseCase _deactivateUseCase;

    public AdminLawAreasController(
        CreateLawAreaUseCase createUseCase,
        UpdateLawAreaUseCase updateUseCase,
        DeactivateLawAreaUseCase deactivateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _deactivateUseCase = deactivateUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LawAreaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLawAreaCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _createUseCase.ExecuteAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(LawAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateLawAreaCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _updateUseCase.ExecuteAsync(command with { Id = id }, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _deactivateUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
