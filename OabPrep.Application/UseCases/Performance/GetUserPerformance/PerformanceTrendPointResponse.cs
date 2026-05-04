namespace OabPrep.Application.UseCases.Performance.GetUserPerformance;

public record PerformanceTrendPointResponse(DateOnly Date, decimal AccuracyPct, int QuestionsAnswered);
