namespace OabPrep.Application.UseCases.Sessions.GetSession;

public record SessionQuestionStateResponse(
    int QuestionId,
    string Statement,
    string LawAreaName,
    int? AnsweredAlternativeId,
    bool IsMarkedForReview,
    IList<SessionAlternativeStateResponse> Alternatives);
