namespace OabPrep.Application.UseCases.Admin.Reports.GetSystemSummary;

public record WeakAreaItem(string AreaName, decimal ErrorRatePct);
public record RegistrationsByMonthItem(int Year, int Month, int Count);

public record SystemSummaryResponse(
    int TotalUsers,
    int ActiveUsersLast30d,
    int TotalQuestions,
    int TotalSessions,
    decimal AvgAccuracyGlobal,
    IList<WeakAreaItem> TopWeakAreas,
    IList<RegistrationsByMonthItem> RegistrationsByMonth);
