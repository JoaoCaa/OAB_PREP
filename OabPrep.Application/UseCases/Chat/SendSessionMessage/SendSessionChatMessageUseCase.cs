using Microsoft.Extensions.Logging;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Chat.SendSessionMessage;

public sealed class SendSessionChatMessageUseCase
{
    private const int MaxMessages = 20;
    private const int AiTimeoutSeconds = 15;

    private const string SystemPromptTemplate =
        "Você é um assistente jurídico especializado no Direito Brasileiro e no Exame OAB. " +
        "Responda SEMPRE em português. Cite artigos de lei específicos quando relevante. " +
        "Questão: {0}\nAlternativas:\n{1}\nÁrea: {2}. Referências legais: {3}.{4}";

    private const string AntiSpoilerClause =
        " NUNCA revele ou insinue qual é a alternativa correta, pois a questão ainda não foi respondida.";

    private readonly IApplicationDbContext _context;
    private readonly ISessionRepository _sessionRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ILlmService _llmService;
    private readonly ILogger<SendSessionChatMessageUseCase> _logger;

    public SendSessionChatMessageUseCase(
        IApplicationDbContext context,
        ISessionRepository sessionRepository,
        IChatRepository chatRepository,
        ILlmService llmService,
        ILogger<SendSessionChatMessageUseCase> logger)
    {
        _context = context;
        _sessionRepository = sessionRepository;
        _chatRepository = chatRepository;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<SendSessionChatMessageResponse> ExecuteAsync(
        int sessionId,
        int questionId,
        SendSessionChatMessageCommand command,
        Guid userId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.FindByIdWithAnswersAsync(sessionId, ct)
            ?? throw new NotFoundException("Sessão", sessionId);

        if (session.UserId != userId)
            throw new ForbiddenException();

        if (!session.Answers.Any(a => a.QuestionId == questionId))
            throw new NotFoundException("Questão na sessão", questionId);

        var count = await _chatRepository.CountAsync(userId, sessionId, questionId, ct);
        if (count >= MaxMessages)
            throw new ChatLimitExceededException(MaxMessages);

        var questionContext = await _chatRepository.GetQuestionContextAsync(questionId, sessionId, ct)
            ?? throw new NotFoundException("Questão", questionId);

        var systemPrompt = BuildSystemPrompt(questionContext);

        var history = await _chatRepository.GetHistoryAsync(userId, sessionId, questionId, MaxMessages, ct);
        var contextMessages = history
            .Select(m => new LlmMessage(m.Role, m.Content))
            .Append(new LlmMessage("user", command.Message))
            .ToList();

        var userMsg = ChatMessage.CreateUser(userId, sessionId, questionId, command.Message);
        await _chatRepository.AddAsync(userMsg, ct);
        await _context.SaveChangesAsync(ct);

        LlmResponse llmResponse;
        using (var aiCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            aiCts.CancelAfter(TimeSpan.FromSeconds(AiTimeoutSeconds));
            try
            {
                llmResponse = await _llmService.SendMessageAsync(
                    new LlmRequest(systemPrompt, contextMessages), aiCts.Token);

                _logger.LogInformation(
                    "Chat IA: session={SessionId} question={QuestionId} tokens={Tokens}",
                    sessionId, questionId, llmResponse.TokensUsed);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new LlmUnavailableException(
                    "O assistente jurídico não respondeu a tempo. Tente novamente em instantes.");
            }
            catch (LlmUnavailableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LlmUnavailableException(
                    "O assistente jurídico está temporariamente indisponível. Tente novamente em instantes.", ex);
            }
        }

        var assistantMsg = ChatMessage.CreateAssistant(
            userId, sessionId, questionId,
            llmResponse.Content, llmResponse.TokensUsed, llmResponse.LegalRefsExtracted);

        await _chatRepository.AddAsync(assistantMsg, ct);
        await _context.SaveChangesAsync(ct);

        return new SendSessionChatMessageResponse(
            assistantMsg.Id,
            llmResponse.Content,
            llmResponse.LegalRefsExtracted,
            llmResponse.TokensUsed);
    }

    private static string BuildSystemPrompt(QuestionContext ctx)
    {
        var alternativesText = string.Join("\n", ctx.Alternatives.Select(a => $"{a.Letter}) {a.Text}"));
        var correctInfo = ctx.IsAnsweredInSession
            ? $" A alternativa correta é {ctx.CorrectLetter}."
            : AntiSpoilerClause;

        return string.Format(
            SystemPromptTemplate,
            ctx.Statement,
            alternativesText,
            ctx.AreaName,
            ctx.LegalRefs ?? "não especificadas",
            correctInfo);
    }
}
