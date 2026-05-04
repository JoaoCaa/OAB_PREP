namespace OabPrep.Application.UseCases.Performance.GetUserPerformance;

public record GetUserPerformanceResponse(
    PerformanceGlobalResponse Global,
    IList<PerformanceByAreaResponse> ByArea,
    IList<PerformanceTrendPointResponse> Trend);
