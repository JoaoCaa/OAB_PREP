namespace OabPrep.Application.Common.Interfaces;

public record WeakAreaData(string AreaName, decimal ErrorRatePct);
public record RegistrationsByMonthData(int Year, int Month, int Count);

public record SystemSummaryData(
    int TotalUsers,
    int ActiveUsersLast30d,
    int TotalQuestions,
    int TotalSessions,
    decimal AvgAccuracyGlobal,
    IList<WeakAreaData> TopWeakAreas,
    IList<RegistrationsByMonthData> RegistrationsByMonth);

public record QuestionStatData(
    int QuestionId,
    string Statement,
    string LawAreaName,
    int ErrorCount,
    int TotalAnswered,
    decimal ErrorRatePct,
    decimal AvgTimeSeconds);

public interface IReportRepository
{
    Task<SystemSummaryData> GetSystemSummaryDataAsync(CancellationToken cancellationToken = default);
    Task<IList<QuestionStatData>> GetQuestionStatsAsync(int? areaId, CancellationToken cancellationToken = default);
}
