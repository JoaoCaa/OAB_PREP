using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.LawAreas.GetLawAreaById;

public sealed class GetLawAreaByIdUseCase
{
    private readonly ILawAreaRepository _repository;

    public GetLawAreaByIdUseCase(ILawAreaRepository repository) => _repository = repository;

    public async Task<LawAreaResponse> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var area = await _repository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException("LawArea", id);

        return new LawAreaResponse(area.Id, area.Name, area.Slug, area.Description, area.IconUrl, 0, null);
    }
}
