using System.Security.Cryptography;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Auth.ConfirmEmail;

public sealed class ConfirmEmailUseCase
{
    private const string DeepLink = "oabprep://email-confirmed";

    private readonly IEmailTokenRepository _emailTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public ConfirmEmailUseCase(
        IEmailTokenRepository emailTokenRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _emailTokenRepository = emailTokenRepository;
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<ConfirmEmailResponse> ExecuteAsync(string rawToken, CancellationToken ct = default)
    {
        var hashedToken = TryHashToken(rawToken)
            ?? throw new InvalidTokenException();

        // Busca token não utilizado (UsedAt IS NULL) do tipo EmailConfirm
        var emailToken = await _emailTokenRepository.FindUnusedByHashAsync(
            hashedToken, TokenType.EmailConfirm, ct)
            ?? throw new InvalidTokenException();

        // Verifica expiração — falha com a mesma mensagem genérica (RNF: sem vazamento de motivo)
        if (emailToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidTokenException();

        var user = await _userRepository.FindByIdAsync(emailToken.UserId, ct)
            ?? throw new InvalidTokenException();

        // Atualiza estado — todas as entidades são rastreadas pelo DbContext
        user.ConfirmEmail();
        emailToken.MarkAsUsed();

        // Registra auditoria no mesmo contexto (commit atômico abaixo)
        await _auditLogService.LogAsync(user.Id, "EMAIL_CONFIRMED", cancellationToken: ct);

        // Persiste Users + EmailTokens + AuditLogs em uma única transação
        await _context.SaveChangesAsync(ct);

        return new ConfirmEmailResponse(DeepLink);
    }

    /// <summary>
    /// Reconstrói o hash armazenado no banco a partir do rawToken recebido na URL.
    /// Retorna null se o rawToken não for um hex válido de 64 caracteres.
    /// </summary>
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
