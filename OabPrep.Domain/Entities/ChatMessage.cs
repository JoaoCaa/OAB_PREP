using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class ChatMessage : BaseEntity<int>
{
    private ChatMessage() { }

    public Guid UserId { get; private set; }
    public int? SessionId { get; private set; }
    public int? QuestionId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int? TokensUsed { get; private set; }
    public string? LegalRefsJson { get; private set; }

    public static ChatMessage CreateUser(Guid userId, int? sessionId, int? questionId, string content) => new()
    {
        UserId = userId,
        SessionId = sessionId,
        QuestionId = questionId,
        Role = "user",
        Content = content,
        CreatedAt = DateTime.UtcNow
    };

    public static ChatMessage CreateAssistant(Guid userId, int? sessionId, int? questionId, string content, int tokensUsed, string[] legalRefs) => new()
    {
        UserId = userId,
        SessionId = sessionId,
        QuestionId = questionId,
        Role = "assistant",
        Content = content,
        TokensUsed = tokensUsed,
        LegalRefsJson = legalRefs.Length > 0 ? System.Text.Json.JsonSerializer.Serialize(legalRefs) : null,
        CreatedAt = DateTime.UtcNow
    };
}
