using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Admin.Reports.GetQuestionStats;

public sealed class GetQuestionStatsUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetQuestionStatsUseCase(IReportRepository reportRepository) =>
        _reportRepository = reportRepository;

    public async Task<QuestionStatsResponse> ExecuteAsync(int? areaId, CancellationToken ct = default)
    {
        var data = await _reportRepository.GetQuestionStatsAsync(areaId, ct);

        var items = data.Select(d => new QuestionStatItem(
            d.QuestionId,
            d.Statement.Length > 120 ? d.Statement[..120] + "…" : d.Statement,
            d.LawAreaName,
            d.ErrorCount,
            d.TotalAnswered,
            d.ErrorRatePct,
            d.AvgTimeSeconds))
            .ToList();

        return new QuestionStatsResponse(items);
    }
}
