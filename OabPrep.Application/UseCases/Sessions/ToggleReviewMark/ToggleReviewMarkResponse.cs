namespace OabPrep.Application.UseCases.Sessions.ToggleReviewMark;

public record ToggleReviewMarkResponse(int SessionId, int QuestionId, bool IsMarkedForReview);
