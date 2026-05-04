namespace OabPrep.Application.UseCases.Questions;

public record QuestionSummaryResponse(
    int Id,
    int LawAreaId,
    string LawAreaName,
    string Statement,
    int Year,
    string? ExamEdition,
    int Difficulty,
    bool IsActive);
