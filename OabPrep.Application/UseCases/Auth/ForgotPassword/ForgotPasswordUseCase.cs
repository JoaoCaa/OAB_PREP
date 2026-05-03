using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Auth.ForgotPassword;

public sealed class ForgotPasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailTokenRepository _emailTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IPasswordResetRateLimitService _rateLimitService;
    private readonly IApplicationDbContext _context;

    public ForgotPasswordUseCase(
        IUserRepository userRepository,
        IEmailTokenRepository emailTokenRepository,
        IEmailService emailService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IPasswordResetRateLimitService rateLimitService,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _emailTokenRepository = emailTokenRepository;
        _emailService = emailService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _rateLimitService = rateLimitService;
        _context = context;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(
        ForgotPasswordCommand command,
        CancellationToken ct = default)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();

        if (_rateLimitService.IsRateLimited(normalizedEmail, out var retryAfter))
            throw new RateLimitExceededException(retryAfter);

        _rateLimitService.RecordAttempt(normalizedEmail);

        var user = await _userRepository.FindByEmailAsync(normalizedEmail, ct);

        if (user is not null)
        {
            // Invalida tokens de reset anteriores do mesmo usuário (executa UPDATE direto)
            await _emailTokenRepository.InvalidatePreviousByUserIdAsync(
                user.Id, TokenType.PasswordReset, ct);

            var (emailToken, rawToken) = EmailToken.CreatePasswordReset(user.Id);
            await _emailTokenRepository.AddAsync(emailToken, ct);
            await _context.SaveChangesAsync(ct);

            var userName = user.Name;
            var userEmail = user.Email;
            _backgroundTaskQueue.Enqueue((sp, bgCt) =>
            {
                var emailSvc = (IEmailService)sp.GetService(typeof(IEmailService))!;
                return emailSvc.SendPasswordResetEmailAsync(userEmail, userName, rawToken, bgCt);
            });
        }

        // Sempre retorna a mesma mensagem — nunca revela se o e-mail existe (RN09)
        return new ForgotPasswordResponse();
    }
}
