namespace OabPrep.Application.UseCases.Sessions.CreateSession;

public record CreateSessionResponse(
    int SessionId,
    int TotalQuestions,
    IReadOnlyList<SessionQuestionResponse> Questions);
