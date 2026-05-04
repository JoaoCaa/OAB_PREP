namespace OabPrep.Application.UseCases.Performance.GetUserPerformance;

public record PerformanceByAreaResponse(
    int LawAreaId,
    string LawAreaName,
    int TotalAnswered,
    int TotalCorrect,
    decimal AccuracyPct);
