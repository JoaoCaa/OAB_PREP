using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Performance.GetUserPerformance;

public sealed class GetUserPerformanceUseCase
{
    private readonly IUserPerformanceCacheRepository _cacheRepository;
    private readonly ISessionRepository _sessionRepository;

    public GetUserPerformanceUseCase(
        IUserPerformanceCacheRepository cacheRepository,
        ISessionRepository sessionRepository)
    {
        _cacheRepository = cacheRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<GetUserPerformanceResponse> ExecuteAsync(
        Guid userId,
        string period,
        CancellationToken ct = default)
    {
        var since = period switch
        {
            "7d"  => (DateTime?)DateTime.UtcNow.AddDays(-7),
            "30d" => (DateTime?)DateTime.UtcNow.AddDays(-30),
            "all" => null,
            _     => throw new ArgumentException($"Período inválido: '{period}'. Use 7d, 30d ou all.")
        };

        var cacheEntries = await _cacheRepository.GetByUserIdAsync(userId, ct);
        var globalCache = cacheEntries.FirstOrDefault(c => !c.LawAreaId.HasValue);
        var byAreaCache = cacheEntries.Where(c => c.LawAreaId.HasValue).ToList();

        var (totalSessions, avgTime) = await _sessionRepository.GetSummaryStatsAsync(userId, ct);
        var trendData = await _sessionRepository.GetTrendAsync(userId, since, ct);

        var global = new PerformanceGlobalResponse(
            globalCache?.TotalAnswered ?? 0,
            globalCache?.CorrectAnswers ?? 0,
            globalCache?.AccuracyPct ?? 0m,
            totalSessions,
            avgTime);

        var byArea = byAreaCache
            .Select(c => new PerformanceByAreaResponse(
                c.LawAreaId!.Value, c.LawAreaName, c.TotalAnswered, c.CorrectAnswers, c.AccuracyPct))
            .ToList();

        var trend = trendData
            .Select(t =>
            {
                var pct = t.Total > 0 ? Math.Round((decimal)t.Correct / t.Total * 100, 2) : 0m;
                return new PerformanceTrendPointResponse(t.Date, pct, t.Total);
            })
            .ToList();

        return new GetUserPerformanceResponse(global, byArea, trend);
    }
}
