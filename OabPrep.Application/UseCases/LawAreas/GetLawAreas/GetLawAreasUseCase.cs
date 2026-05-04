using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.LawAreas.GetLawAreas;

public sealed class GetLawAreasUseCase
{
    internal const string CacheKey = "law-areas-public-list";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly ILawAreaRepository _repository;
    private readonly IMemoryCache _cache;

    public GetLawAreasUseCase(ILawAreaRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IList<LawAreaResponse>> ExecuteAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out IList<LawAreaResponse>? cached) && cached is not null)
            return cached;

        var areas = await _repository.GetAllActiveAsync(ct);

        var result = areas
            .Select(a => new LawAreaResponse(a.Id, a.Name, a.Slug, a.Description, a.IconUrl, 0, null))
            .ToList();

        _cache.Set(CacheKey, result, CacheDuration);

        return result;
    }
}
