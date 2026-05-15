using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Sessions.SubmitAnswer;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Sessions;

public sealed class SubmitAnswerUseCaseTests
{
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IQuestionRepository> _questionRepo = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly SubmitAnswerUseCase _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public SubmitAnswerUseCaseTests()
    {
        _sut = new SubmitAnswerUseCase(_sessionRepo.Object, _questionRepo.Object, _context.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SessionNotFound_ThrowsNotFoundException()
    {
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        await _sut.Invoking(s => s.ExecuteAsync(new SubmitAnswerCommand(1, 1, 1, null), _userId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongUser_ThrowsForbiddenException()
    {
        var session = Session.Create(Guid.NewGuid(), [1]);
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _sut.Invoking(s => s.ExecuteAsync(new SubmitAnswerCommand(1, 1, 1, null), _userId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ExecuteAsync_SessionNotInProgress_ThrowsArgumentException()
    {
        var session = Session.Create(_userId, [1]);
        session.Complete();
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _sut.Invoking(s => s.ExecuteAsync(new SubmitAnswerCommand(1, 1, 1, null), _userId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*progresso*");
    }

    [Fact]
    public async Task ExecuteAsync_QuestionNotInSession_ThrowsArgumentException()
    {
        var session = Session.Create(_userId, [99]); // question 99, not 1
        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _sut.Invoking(s => s.ExecuteAsync(new SubmitAnswerCommand(1, 1, 1, null), _userId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Questão não pertence*");
    }

    [Fact]
    public async Task ExecuteAsync_CorrectAnswer_ReturnsIsCorrectTrue()
    {
        var question = Fakers.ValidQuestion();
        var correctAlt = question.Alternatives.First(a => a.IsCorrect);
        var session = Session.Create(_userId, [question.Id]);

        _sessionRepo.Setup(r => r.FindByIdWithAnswersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _questionRepo.Setup(r => r.FindByIdWithAlternativesAsync(question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ExecuteAsync(
            new SubmitAnswerCommand(1, question.Id, correctAlt.Id, null), _userId);

        result.IsCorrect.Should().BeTrue();
    }
}
