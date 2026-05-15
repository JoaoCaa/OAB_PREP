using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Auth;

public sealed class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<ILoginAttemptService> _loginAttempt = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _sut = new LoginUseCase(
            _userRepo.Object, _refreshRepo.Object, _hasher.Object,
            _jwtService.Object, _loginAttempt.Object, _audit.Object, _context.Object);
    }

    private void SetupSuccess(User user)
    {
        _loginAttempt.Setup(l => l.IsLockedOut(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(false);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<TimeSpan>())).Returns("access.token");
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task ExecuteAsync_AccountLockedOut_ThrowsAccountLockedException()
    {
        var remaining = TimeSpan.FromMinutes(5);
        _loginAttempt.Setup(l => l.IsLockedOut(It.IsAny<string>(), out remaining)).Returns(true);

        await _sut.Invoking(s => s.ExecuteAsync(new LoginCommand("a@a.com", "p")))
            .Should().ThrowAsync<AccountLockedException>();
    }

    [Fact]
    public async Task ExecuteAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        _loginAttempt.Setup(l => l.IsLockedOut(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(false);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await _sut.Invoking(s => s.ExecuteAsync(new LoginCommand("nobody@a.com", "pw")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = Fakers.ConfirmedUser();
        _loginAttempt.Setup(l => l.IsLockedOut(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(false);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await _sut.Invoking(s => s.ExecuteAsync(new LoginCommand(user.Email, "wrong")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_EmailNotConfirmed_ThrowsUnauthorizedException()
    {
        var user = Fakers.ActiveUser(); // not confirmed
        _loginAttempt.Setup(l => l.IsLockedOut(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(false);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        await _sut.Invoking(s => s.ExecuteAsync(new LoginCommand(user.Email, "Abc@1234")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var user = Fakers.ConfirmedUser();
        SetupSuccess(user);

        var result = await _sut.ExecuteAsync(new LoginCommand(user.Email, "Abc@1234", RememberMe: false));

        result.AccessToken.Should().Be("access.token");
        result.UserId.Should().Be(user.Id);
        result.ExpiresIn.Should().Be((int)TimeSpan.FromHours(8).TotalSeconds);
    }

    [Fact]
    public async Task ExecuteAsync_RememberMe_TokenExpiresIn30Days()
    {
        var user = Fakers.ConfirmedUser();
        SetupSuccess(user);

        var result = await _sut.ExecuteAsync(new LoginCommand(user.Email, "p", RememberMe: true));

        result.ExpiresIn.Should().Be((int)TimeSpan.FromDays(30).TotalSeconds);
    }
}
