using FluentAssertions;
using Moq;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Auth.OAuthGoogle;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Auth;

public sealed class OAuthGoogleUseCaseTests
{
    private readonly Mock<IGoogleOAuthService> _googleOAuth = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly OAuthGoogleUseCase _sut;

    public OAuthGoogleUseCaseTests()
    {
        _sut = new OAuthGoogleUseCase(
            _googleOAuth.Object, _userRepo.Object, _refreshRepo.Object,
            _jwtService.Object, _audit.Object, _context.Object);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidToken_ThrowsUnauthorizedException()
    {
        _googleOAuth.Setup(g => g.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException());

        await _sut.Invoking(s => s.ExecuteAsync(new OAuthGoogleCommand("bad.token")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_NewUser_CreatesUserWithEmailConfirmed()
    {
        var googleInfo = new GoogleUserInfo("new@google.com", "Google User", "sub123");
        _googleOAuth.Setup(g => g.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleInfo);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<TimeSpan>())).Returns("jwt");
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ExecuteAsync(new OAuthGoogleCommand("valid.token"));

        result.AccessToken.Should().Be("jwt");
        _userRepo.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "new@google.com" && u.EmailConfirmed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingUser_DoesNotCreateNewUser()
    {
        var existingUser = Fakers.ConfirmedUser("existing@google.com");
        var googleInfo = new GoogleUserInfo("existing@google.com", "User", "sub456");
        _googleOAuth.Setup(g => g.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleInfo);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<TimeSpan>())).Returns("jwt");
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ExecuteAsync(new OAuthGoogleCommand("valid.token"));

        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_InactiveUser_ThrowsUnauthorizedException()
    {
        var inactiveUser = Fakers.ConfirmedUser();
        inactiveUser.Block();
        var googleInfo = new GoogleUserInfo(inactiveUser.Email, "User", "sub789");
        _googleOAuth.Setup(g => g.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleInfo);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        await _sut.Invoking(s => s.ExecuteAsync(new OAuthGoogleCommand("valid.token")))
            .Should().ThrowAsync<UnauthorizedException>();
    }
}
