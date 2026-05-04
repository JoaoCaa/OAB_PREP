namespace OabPrep.Application.UseCases.Sessions.SubmitAnswer;

public record AnswerAlternativeResponse(
    int AlternativeId,
    string Letter,
    string Text,
    bool IsCorrect,
    string Explanation);
