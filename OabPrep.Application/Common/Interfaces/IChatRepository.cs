using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public record AlternativeInfo(string Letter, string Text);

public record QuestionContext(
    string Statement,
    string AreaName,
    string? LegalRefs,
    bool IsAnsweredInSession,
    IList<AlternativeInfo> Alternatives,
    string CorrectLetter);

public interface IChatRepository
{
    Task<IList<ChatMessage>> GetHistoryAsync(Guid userId, int? sessionId, int? questionId, int limit, CancellationToken ct = default);
    Task<int> CountAsync(Guid userId, int sessionId, int questionId, CancellationToken ct = default);
    Task<QuestionContext?> GetQuestionContextAsync(int questionId, int? sessionId, CancellationToken ct = default);
    Task AddAsync(ChatMessage message, CancellationToken ct = default);
}
