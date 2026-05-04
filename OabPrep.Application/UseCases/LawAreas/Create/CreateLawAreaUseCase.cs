using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.LawAreas.GetLawAreas;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.LawAreas.Create;

public sealed class CreateLawAreaUseCase
{
    private readonly ILawAreaRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public CreateLawAreaUseCase(
        ILawAreaRepository repository,
        IApplicationDbContext context,
        IMemoryCache cache)
    {
        _repository = repository;
        _context = context;
        _cache = cache;
    }

    public async Task<LawAreaResponse> ExecuteAsync(CreateLawAreaCommand command, CancellationToken ct = default)
    {
        var area = LawArea.Create(command.Name, command.Description, command.IconUrl);
        await _repository.AddAsync(area, ct);
        await _context.SaveChangesAsync(ct);

        _cache.Remove(GetLawAreasUseCase.CacheKey);

        return new LawAreaResponse(area.Id, area.Name, area.Slug, area.Description, area.IconUrl, 0, null);
    }
}
