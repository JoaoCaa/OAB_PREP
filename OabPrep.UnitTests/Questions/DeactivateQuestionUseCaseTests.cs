using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Questions.Deactivate;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Questions;

public sealed class DeactivateQuestionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _repo = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly DeactivateQuestionUseCase _sut;

    public DeactivateQuestionUseCaseTests()
    {
        _sut = new DeactivateQuestionUseCase(_repo.Object, _audit.Object, _context.Object);
    }

    [Fact]
    public async Task ExecuteAsync_QuestionNotFound_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.FindByIdWithAlternativesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        await _sut.Invoking(s => s.ExecuteAsync(99, Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_DeactivatesQuestion()
    {
        var question = Fakers.ValidQuestion();
        _repo.Setup(r => r.FindByIdWithAlternativesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ExecuteAsync(1, Guid.NewGuid());

        question.IsActive.Should().BeFalse();
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
