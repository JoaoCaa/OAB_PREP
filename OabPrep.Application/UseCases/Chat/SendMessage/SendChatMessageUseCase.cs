using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Chat.SendMessage;

public sealed class SendChatMessageUseCase
{
    private const string SystemPromptTemplate =
        "Você é um assistente jurídico especializado no Direito Brasileiro e no Exame OAB. " +
        "Responda SEMPRE em português. Cite artigos de lei específicos. " +
        "Questão: {0}\nAlternativas:\n{1}\nÁrea: {2}. Referências: {3}.{4}";

    private const string GenericSystemPrompt =
        "Você é um assistente jurídico especializado no Direito Brasileiro e no Exame OAB. " +
        "Responda SEMPRE em português. Cite artigos de lei específicos.";

    private const int HistoryLimit = 20;

    private readonly IApplicationDbContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly ILlmService _llmService;

    public SendChatMessageUseCase(
        IApplicationDbContext context,
        IChatRepository chatRepository,
        ILlmService llmService)
    {
        _context = context;
        _chatRepository = chatRepository;
        _llmService = llmService;
    }

    public async Task<SendChatMessageResponse> ExecuteAsync(
        SendChatMessageCommand command,
        Guid userId,
        CancellationToken ct = default)
    {
        var systemPrompt = await BuildSystemPromptAsync(command, ct);

        var history = await _chatRepository.GetHistoryAsync(
            userId, command.SessionId, command.QuestionId, HistoryLimit, ct);

        var messages = history
            .Select(m => new LlmMessage(m.Role, m.Content))
            .Append(new LlmMessage("user", command.UserMessage))
            .ToList();

        LlmResponse llmResponse;
        try
        {
            llmResponse = await _llmService.SendMessageAsync(
                new LlmRequest(systemPrompt, messages), ct);
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

        var userMsg = ChatMessage.CreateUser(userId, command.SessionId, command.QuestionId, command.UserMessage);
        var assistantMsg = ChatMessage.CreateAssistant(
            userId, command.SessionId, command.QuestionId,
            llmResponse.Content, llmResponse.TokensUsed, llmResponse.LegalRefsExtracted);

        await _chatRepository.AddAsync(userMsg, ct);
        await _chatRepository.AddAsync(assistantMsg, ct);
        await _context.SaveChangesAsync(ct);

        return new SendChatMessageResponse(
            assistantMsg.Id,
            llmResponse.Content,
            llmResponse.LegalRefsExtracted,
            llmResponse.TokensUsed);
    }

    private async Task<string> BuildSystemPromptAsync(SendChatMessageCommand command, CancellationToken ct)
    {
        if (command.QuestionId is null)
            return GenericSystemPrompt;

        var ctx = await _chatRepository.GetQuestionContextAsync(
            command.QuestionId.Value, command.SessionId, ct);

        if (ctx is null)
            return GenericSystemPrompt;

        var alternativesText = string.Join("\n", ctx.Alternatives.Select(a => $"{a.Letter}) {a.Text}"));

        var correctInfo = ctx.IsAnsweredInSession
            ? $" A alternativa correta é {ctx.CorrectLetter}."
            : " NUNCA revele nem insinue qual é a alternativa correta.";

        return string.Format(SystemPromptTemplate,
            ctx.Statement,
            alternativesText,
            ctx.AreaName,
            ctx.LegalRefs ?? "não especificadas",
            correctInfo);
    }
}
