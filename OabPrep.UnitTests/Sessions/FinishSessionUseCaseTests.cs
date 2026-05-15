using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Sessions.FinishSession;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.UnitTests.Sessions;

public sealed class FinishSessionUseCaseTests
{
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IUserPerformanceCacheRepository> _cacheRepo = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly FinishSessionUseCase _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public FinishSessionUseCaseTests()
    {
        _sut = new FinishSessionUseCase(_sessionRepo.Object, _cacheRepo.Object, _context.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SessionNotFound_ThrowsNotFoundException()
    {
        _sessionRepo.Setup(r => r.FindByIdForFinishAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        await _sut.Invoking(s => s.ExecuteAsync(1, _userId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongUser_ThrowsForbiddenException()
    {
        var session = Session.Create(Guid.NewGuid(), [1, 2]);
        _sessionRepo.Setup(r => r.FindByIdForFinishAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _sut.Invoking(s => s.ExecuteAsync(1, _userId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyFinished_ThrowsConflictException()
    {
        var session = Session.Create(_userId, [1]);
        session.Complete();
        _sessionRepo.Setup(r => r.FindByIdForFinishAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        await _sut.Invoking(s => s.ExecuteAsync(1, _userId))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*finalizada*");
    }

    [Fact]
    public async Task ExecuteAsync_ValidSession_CompletesAndReturnsStats()
    {
        var session = Session.Create(_userId, [1]);
        _sessionRepo.Setup(r => r.FindByIdForFinishAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _sessionRepo.Setup(r => r.GetAreaStatsForUserAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _cacheRepo.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ExecuteAsync(1, _userId);

        result.SessionId.Should().Be(session.Id);
        session.Status.Should().Be(SessionStatus.Completed);
    }
}
