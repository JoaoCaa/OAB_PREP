using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Sessions;

public sealed class CreateSessionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _questionRepo = new();
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly CreateSessionUseCase _sut;

    public CreateSessionUseCaseTests()
    {
        _sut = new CreateSessionUseCase(_questionRepo.Object, _sessionRepo.Object, _context.Object);
    }

    private static CreateSessionCommand Cmd(int count = 5) => new(
        LawAreaIds: null,
        QuestionCount: count,
        ExcludeAnswered: false
    );

    [Fact]
    public async Task ExecuteAsync_NotEnoughQuestions_ThrowsArgumentException()
    {
        _questionRepo.Setup(r => r.GetActiveForSessionAsync(
                It.IsAny<IReadOnlyList<int>?>(),
                It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question> { Fakers.ValidQuestion() });

        await _sut.Invoking(s => s.ExecuteAsync(Cmd(5), Guid.NewGuid()))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*suficientes*");
    }

    [Fact]
    public async Task ExecuteAsync_EnoughQuestions_ReturnsSession()
    {
        var questions = Enumerable.Range(1, 5).Select(_ => Fakers.ValidQuestion()).ToList();
        _questionRepo.Setup(r => r.GetActiveForSessionAsync(
                It.IsAny<IReadOnlyList<int>?>(),
                It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ExecuteAsync(Cmd(5), Guid.NewGuid());

        result.TotalQuestions.Should().Be(5);
        result.Questions.Should().HaveCount(5);
    }
}
