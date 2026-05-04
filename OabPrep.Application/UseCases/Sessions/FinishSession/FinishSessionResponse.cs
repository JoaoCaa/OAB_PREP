namespace OabPrep.Application.UseCases.Sessions.FinishSession;

public record FinishSessionResponse(
    int SessionId,
    int TotalQuestions,
    int CorrectAnswers,
    decimal AccuracyPct,
    int TimeSpentSeconds,
    IReadOnlyList<AreaPerformanceResponse> ByArea,
    IReadOnlyList<string> WeakAreas);
