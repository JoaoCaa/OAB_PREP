namespace OabPrep.Application.UseCases.Questions;

public record QuestionDetailResponse(
    int Id,
    int LawAreaId,
    string LawAreaName,
    string Statement,
    int Year,
    string? ExamEdition,
    string? Explanation,
    IReadOnlyList<string> LegalRefs,
    int Difficulty,
    bool IsActive,
    IReadOnlyList<AlternativeResponse> Alternatives);
