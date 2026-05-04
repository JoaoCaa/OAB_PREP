namespace OabPrep.Application.UseCases.Sessions.SubmitAnswer;

public record SubmitAnswerResponse(
    bool IsCorrect,
    int CorrectAlternativeId,
    string? QuestionExplanation,
    IReadOnlyList<string> LegalRefs,
    IReadOnlyList<AnswerAlternativeResponse> Alternatives);
