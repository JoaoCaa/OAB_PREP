namespace OabPrep.Application.UseCases.LawAreas;

public record LawAreaResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    int QuestionCount,
    double? UserAccuracyPct);
