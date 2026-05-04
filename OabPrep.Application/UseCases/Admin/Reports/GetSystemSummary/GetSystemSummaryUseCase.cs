using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Admin.Reports.GetSystemSummary;

public sealed class GetSystemSummaryUseCase
{
    private const string CacheKey = "admin:reports:summary";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IReportRepository _reportRepository;
    private readonly IMemoryCache _cache;

    public GetSystemSummaryUseCase(IReportRepository reportRepository, IMemoryCache cache)
    {
        _reportRepository = reportRepository;
        _cache = cache;
    }

    public async Task<SystemSummaryResponse> ExecuteAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out SystemSummaryResponse? cached))
            return cached!;

        var data = await _reportRepository.GetSystemSummaryDataAsync(ct);

        var response = new SystemSummaryResponse(
            data.TotalUsers,
            data.ActiveUsersLast30d,
            data.TotalQuestions,
            data.TotalSessions,
            data.AvgAccuracyGlobal,
            data.TopWeakAreas.Select(a => new WeakAreaItem(a.AreaName, a.ErrorRatePct)).ToList(),
            data.RegistrationsByMonth.Select(r => new RegistrationsByMonthItem(r.Year, r.Month, r.Count)).ToList());

        _cache.Set(CacheKey, response, CacheTtl);
        return response;
    }
}
