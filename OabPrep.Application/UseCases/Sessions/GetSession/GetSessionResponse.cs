namespace OabPrep.Application.UseCases.Sessions.GetSession;

public record GetSessionResponse(
    int SessionId,
    string Status,
    int TotalQuestions,
    int AnsweredCount,
    IList<SessionQuestionStateResponse> Questions);
