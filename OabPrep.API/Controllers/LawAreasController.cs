using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.UseCases.LawAreas;
using OabPrep.Application.UseCases.LawAreas.GetLawAreaById;
using OabPrep.Application.UseCases.LawAreas.GetLawAreas;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/law-areas")]
public sealed class LawAreasController : ControllerBase
{
    private readonly GetLawAreasUseCase _getLawAreasUseCase;
    private readonly GetLawAreaByIdUseCase _getLawAreaByIdUseCase;

    public LawAreasController(
        GetLawAreasUseCase getLawAreasUseCase,
        GetLawAreaByIdUseCase getLawAreaByIdUseCase)
    {
        _getLawAreasUseCase = getLawAreasUseCase;
        _getLawAreaByIdUseCase = getLawAreaByIdUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IList<LawAreaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _getLawAreasUseCase.ExecuteAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LawAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _getLawAreaByIdUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(response);
    }
}
