namespace OabPrep.Application.UseCases.Sessions.FinishSession;

public record AreaPerformanceResponse(
    int LawAreaId,
    string LawAreaName,
    int Total,
    int Correct,
    decimal AccuracyPct);
