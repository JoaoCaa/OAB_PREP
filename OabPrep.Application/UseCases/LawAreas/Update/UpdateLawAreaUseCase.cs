using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.LawAreas.GetLawAreas;

namespace OabPrep.Application.UseCases.LawAreas.Update;

public sealed class UpdateLawAreaUseCase
{
    private readonly ILawAreaRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public UpdateLawAreaUseCase(
        ILawAreaRepository repository,
        IApplicationDbContext context,
        IMemoryCache cache)
    {
        _repository = repository;
        _context = context;
        _cache = cache;
    }

    public async Task<LawAreaResponse> ExecuteAsync(UpdateLawAreaCommand command, CancellationToken ct = default)
    {
        var area = await _repository.FindByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("LawArea", command.Id);

        area.Update(command.Name, command.Description, command.IconUrl);
        await _context.SaveChangesAsync(ct);

        _cache.Remove(GetLawAreasUseCase.CacheKey);

        return new LawAreaResponse(area.Id, area.Name, area.Slug, area.Description, area.IconUrl, 0, null);
    }
}
