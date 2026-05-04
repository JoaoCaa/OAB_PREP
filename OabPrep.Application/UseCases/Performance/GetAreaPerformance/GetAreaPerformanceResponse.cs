namespace OabPrep.Application.UseCases.Performance.GetAreaPerformance;

public record GetAreaPerformanceResponse(
    string AreaName,
    int TotalAnswered,
    int TotalCorrect,
    decimal AccuracyPct,
    IList<WrongQuestionResponse> RecentWrongQuestions,
    IList<AreaEvolutionPointResponse> Evolution);
