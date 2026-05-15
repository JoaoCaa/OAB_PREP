using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Chat.SendSessionMessage;
using OabPrep.Domain.Entities;

namespace OabPrep.UnitTests.Chat;

public sealed class SendSessionChatMessageUseCaseTests
{
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IChatRepository> _chatRepo = new();
    private readonly Mock<ILlmService> _llmService = new();
    private readonly Mock<ILogger<SendSessionChatMessageUseCase>> _logger = new();
    private readonly SendSessionChatMessageUseCase _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public SendSessionChatMessageUseCaseTests()
    {
        _sut = new SendSessionChatMessageUseCase(
            _context.Object, _sessionRepo.Object, _chatRepo.Object,
            _llmService.Object, _logger.Object);
    }

    private void SetupBasicSession(Session session)
    {
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    [Fact]
    public async Task ExecuteAsync_SessionNotFound_ThrowsNotFoundException()
    {
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        await _sut.Invoking(s => s.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("Hi"), _userId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongUser_ThrowsForbiddenException()
    {
        var session = Session.Create(Guid.NewGuid(), [1]);
        SetupBasicSession(session);

        await _sut.Invoking(s => s.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("Hi"), _userId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ExecuteAsync_MessageLimitReached_ThrowsChatLimitExceededException()
    {
        var session = Session.Create(_userId, [1]);
        SetupBasicSession(session);
        _chatRepo.Setup(r => r.CountAsync(_userId, It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(20);

        await _sut.Invoking(s => s.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("21st"), _userId))
            .Should().ThrowAsync<ChatLimitExceededException>()
            .Where(e => e.Limit == 20);
    }

    [Fact]
    public async Task ExecuteAsync_LlmTimeout_ThrowsLlmUnavailableException()
    {
        var session = Session.Create(_userId, [1]);
        SetupBasicSession(session);
        _chatRepo.Setup(r => r.CountAsync(_userId, It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _chatRepo.Setup(r => r.GetQuestionContextAsync(1, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionContext("stmt", "Dir. Civil", null, false));
        _chatRepo.Setup(r => r.GetHistoryAsync(_userId, It.IsAny<int>(), 1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _llmService.Setup(l => l.SendMessageAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await _sut.Invoking(s => s.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("Q"), _userId))
            .Should().ThrowAsync<LlmUnavailableException>();
    }

    [Fact]
    public async Task ExecuteAsync_AntiSpoilerApplied_WhenQuestionNotAnswered()
    {
        // Verify that NUNCA revele is included in the system prompt when IsAnsweredInSession = false
        var session = Session.Create(_userId, [1]);
        SetupBasicSession(session);
        _chatRepo.Setup(r => r.CountAsync(_userId, It.IsAny<int>(), 1, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _chatRepo.Setup(r => r.GetQuestionContextAsync(1, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionContext("stmt", "Dir. Civil", null, IsAnsweredInSession: false));
        _chatRepo.Setup(r => r.GetHistoryAsync(_userId, It.IsAny<int>(), 1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        LlmRequest? capturedRequest = null;
        _llmService.Setup(l => l.SendMessageAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .Callback<LlmRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new LlmResponse("answer", 100, []));

        await _sut.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("Q"), _userId);

        capturedRequest!.SystemPrompt.Should().Contain("NUNCA revele");
    }

    [Fact]
    public async Task ExecuteAsync_NoAntiSpoiler_WhenQuestionAlreadyAnswered()
    {
        var session = Session.Create(_userId, [1]);
        SetupBasicSession(session);
        _chatRepo.Setup(r => r.CountAsync(_userId, It.IsAny<int>(), 1, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _chatRepo.Setup(r => r.GetQuestionContextAsync(1, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionContext("stmt", "Dir. Civil", null, IsAnsweredInSession: true));
        _chatRepo.Setup(r => r.GetHistoryAsync(_userId, It.IsAny<int>(), 1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        LlmRequest? capturedRequest = null;
        _llmService.Setup(l => l.SendMessageAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .Callback<LlmRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new LlmResponse("answer", 100, []));

        await _sut.ExecuteAsync(1, 1, new SendSessionChatMessageCommand("Q"), _userId);

        capturedRequest!.SystemPrompt.Should().NotContain("NUNCA revele");
    }
}
