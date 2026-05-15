using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Moq;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.Mappings;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.Domain.Entities;
using OabPrep.UnitTests.Common;

namespace OabPrep.UnitTests.Auth;

public sealed class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmailTokenRepository> _emailTokenRepo = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IBackgroundTaskQueue> _queue = new();
    private readonly IMapper _mapper;
    private readonly RegisterUserUseCase _sut;

    public RegisterUserUseCaseTests()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<AuthProfile>());
        _mapper = cfg.CreateMapper();
        _sut = new RegisterUserUseCase(
            _userRepo.Object, _emailTokenRepo.Object, _context.Object,
            _hasher.Object, _queue.Object, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_EmailAlreadyExists_ThrowsValidationException()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var cmd = new RegisterUserCommand
        {
            Name = "Test", Email = "test@test.com",
            Password = "Abc@1234", ConfirmPassword = "Abc@1234", AcceptedTerms = true
        };

        await _sut.Invoking(s => s.ExecuteAsync(cmd))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*E-mail já cadastrado*");
    }

    [Fact]
    public async Task ExecuteAsync_ValidCommand_CreatesUserAndEnqueuesEmail()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cmd = new RegisterUserCommand
        {
            Name = "João", Email = "joao@test.com",
            Password = "Abc@1234", ConfirmPassword = "Abc@1234", AcceptedTerms = true
        };

        var result = await _sut.ExecuteAsync(cmd);

        result.Message.Should().Contain("Verifique seu e-mail");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _queue.Verify(q => q.Enqueue(It.IsAny<Func<IServiceProvider, CancellationToken, Task>>()), Times.Once);
    }
}
