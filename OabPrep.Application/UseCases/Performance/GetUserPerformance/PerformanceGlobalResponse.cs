namespace OabPrep.Application.UseCases.Performance.GetUserPerformance;

public record PerformanceGlobalResponse(
    int TotalAnswered,
    int TotalCorrect,
    decimal AccuracyPct,
    int TotalSessions,
    decimal AvgTimePerQuestion);
