namespace OabPrep.Application.UseCases.Admin.Reports.GetQuestionStats;

public record QuestionStatItem(
    int QuestionId,
    string Statement,
    string LawAreaName,
    int ErrorCount,
    int TotalAnswered,
    decimal ErrorRatePct,
    decimal AvgTimeSeconds);

public record QuestionStatsResponse(IList<QuestionStatItem> Questions);
