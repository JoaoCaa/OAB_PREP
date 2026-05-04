namespace OabPrep.Application.UseCases.Performance.GetAreaPerformance;

public record AreaEvolutionPointResponse(DateOnly Date, decimal AccuracyPct, int QuestionsAnswered);
