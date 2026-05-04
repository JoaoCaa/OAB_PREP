namespace OabPrep.Application.UseCases.Sessions;

public record SessionQuestionResponse(
    int QuestionId,
    string Statement,
    int Year,
    string? ExamEdition,
    string LawAreaName,
    int Difficulty,
    IReadOnlyList<SessionAlternativeResponse> Alternatives);
