using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Questions;
using OabPrep.Application.UseCases.Questions.Create;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.UnitTests.Questions;

public sealed class CreateQuestionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _repo = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly CreateQuestionUseCase _sut;

    public CreateQuestionUseCaseTests()
    {
        _sut = new CreateQuestionUseCase(_repo.Object, _audit.Object, _context.Object);
    }

    private static CreateQuestionCommand ValidCommand() => new(
        LawAreaId: 1,
        Statement: "Qual é o prazo?",
        Year: 2024,
        ExamEdition: "1ª Fase",
        Explanation: "Conforme o art. 1º",
        LegalRefs: ["art. 1º CC"],
        Difficulty: (int)DifficultyLevel.Medium,
        Alternatives: Enumerable.Range(0, 5).Select((_, i) => new AlternativeCommandItem(
            $"Alternativa {i}",
            i == 0,
            $"Explicação {i}")).ToList()
    );

    [Fact]
    public async Task ExecuteAsync_ValidCommand_ReturnsDetail()
    {
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ExecuteAsync(ValidCommand(), Guid.NewGuid());

        result.Should().NotBeNull();
        result.Statement.Should().Be("Qual é o prazo?");
        result.Alternatives.Should().HaveCount(5);
        _repo.Verify(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(new[] { "art. 1º CC", "art. 2º CC" }, "art. 1º CC|art. 2º CC")]
    [InlineData(null, null)]
    [InlineData(new string[0], null)]
    public void ToLegalRefsString_ConvertsCorrectly(string[]? refs, string? expected)
    {
        var result = CreateQuestionUseCase.ToLegalRefsString(refs);
        result.Should().Be(expected);
    }
}
