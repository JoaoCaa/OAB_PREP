namespace OabPrep.Application.UseCases.Sessions.CreateSession;

public record CreateSessionCommand(
    IReadOnlyList<int>? LawAreaIds,
    int QuestionCount,
    bool ExcludeAnswered);
