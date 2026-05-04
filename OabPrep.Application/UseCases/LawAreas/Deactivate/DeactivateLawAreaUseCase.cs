using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.LawAreas.GetLawAreas;

namespace OabPrep.Application.UseCases.LawAreas.Deactivate;

public sealed class DeactivateLawAreaUseCase
{
    private readonly ILawAreaRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public DeactivateLawAreaUseCase(
        ILawAreaRepository repository,
        IApplicationDbContext context,
        IMemoryCache cache)
    {
        _repository = repository;
        _context = context;
        _cache = cache;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var area = await _repository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException("LawArea", id);

        area.Deactivate();
        await _context.SaveChangesAsync(ct);

        _cache.Remove(GetLawAreasUseCase.CacheKey);
    }
}
