using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using System.Text.Json;

namespace OabPrep.Application.UseCases.Chat.GetHistory;

public sealed class GetChatHistoryUseCase
{
    private const int MaxMessages = 20;

    private readonly ISessionRepository _sessionRepository;
    private readonly IChatRepository _chatRepository;

    public GetChatHistoryUseCase(ISessionRepository sessionRepository, IChatRepository chatRepository)
    {
        _sessionRepository = sessionRepository;
        _chatRepository = chatRepository;
    }

    public async Task<GetChatHistoryResponse> ExecuteAsync(
        int sessionId, int questionId, Guid userId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.FindByIdWithAnswersAsync(sessionId, ct)
            ?? throw new NotFoundException("Sessão", sessionId);

        if (session.UserId != userId)
            throw new ForbiddenException();

        if (!session.Answers.Any(a => a.QuestionId == questionId))
            throw new NotFoundException("Questão na sessão", questionId);

        var questionContext = await _chatRepository.GetQuestionContextAsync(questionId, sessionId, ct)
            ?? throw new NotFoundException("Questão", questionId);

        var messages = await _chatRepository.GetHistoryAsync(userId, sessionId, questionId, MaxMessages, ct);

        var items = messages.Select(m => new ChatMessageItem(
            m.Id,
            m.Role,
            m.Content,
            m.CreatedAt,
            DeserializeLegalRefs(m.LegalRefsJson))).ToList();

        return new GetChatHistoryResponse(
            new ChatQuestionContext(questionContext.Statement, questionContext.AreaName, questionContext.LegalRefs),
            items,
            items.Count,
            MaxMessages);
    }

    private static string[] DeserializeLegalRefs(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<string[]>(json) ?? []; }
        catch { return []; }
    }
}
