using System.Security.Cryptography;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Auth.ResetPassword;

public sealed class ResetPasswordUseCase
{
    private readonly IEmailTokenRepository _emailTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public ResetPasswordUseCase(
        IEmailTokenRepository emailTokenRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _emailTokenRepository = emailTokenRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<ResetPasswordResponse> ExecuteAsync(
        ResetPasswordCommand command,
        CancellationToken ct = default)
    {
        var hashedToken = TryHashToken(command.Token)
            ?? throw new InvalidTokenException();

        var emailToken = await _emailTokenRepository.FindUnusedByHashAsync(
            hashedToken, TokenType.PasswordReset, ct)
            ?? throw new InvalidTokenException();

        if (emailToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidTokenException();

        var user = await _userRepository.FindByIdAsync(emailToken.UserId, ct)
            ?? throw new InvalidTokenException();

        // Verifica se nova senha é igual à atual (RN10)
        var isSamePassword = await Task.Run(
            () => _passwordHasher.Verify(command.NewPassword, user.PasswordHash), ct);

        if (isSamePassword)
            throw new SamePasswordException();

        var newHash = await Task.Run(
            () => _passwordHasher.Hash(command.NewPassword), ct);

        // Todas as mudanças abaixo são rastreadas pelo DbContext — commit atômico único
        user.UpdatePassword(newHash);
        emailToken.MarkAsUsed();
        await _refreshTokenRepository.MarkAllAsRevokedAsync(user.Id, ct);
        await _auditLogService.LogAsync(user.Id, "PASSWORD_RESET", cancellationToken: ct);

        await _context.SaveChangesAsync(ct);

        return new ResetPasswordResponse();
    }

    private static string? TryHashToken(string? rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length != 64)
            return null;

        try
        {
            var rawBytes = Convert.FromHexString(rawToken);
            return Convert.ToHexString(SHA256.HashData(rawBytes));
        }
        catch
        {
            return null;
        }
    }
}
