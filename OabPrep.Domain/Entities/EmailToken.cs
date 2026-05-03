using System.Security.Cryptography;
using OabPrep.Domain.Common;
using OabPrep.Domain.Enums;

namespace OabPrep.Domain.Entities;

public sealed class EmailToken : BaseEntity<int>
{
    private EmailToken() { }

    /// <summary>
    /// Retorna a entidade (com Token = SHA256(rawBytes) para persistência)
    /// e o rawToken (hex dos bytes aleatórios) para enviar no link do e-mail.
    /// O banco NUNCA armazena o token cru — apenas o hash.
    /// </summary>
    public static (EmailToken Entity, string RawToken) CreateConfirmation(Guid userId)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(rawBytes);
        var storedToken = Convert.ToHexString(SHA256.HashData(rawBytes));

        var entity = new EmailToken
        {
            UserId = userId,
            Token = storedToken,
            TokenType = TokenType.EmailConfirm,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        return (entity, rawToken);
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public TokenType TokenType { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public User? User { get; private set; }

    public static (EmailToken Entity, string RawToken) CreatePasswordReset(Guid userId)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(rawBytes);
        var storedToken = Convert.ToHexString(SHA256.HashData(rawBytes));

        return (new EmailToken
        {
            UserId = userId,
            Token = storedToken,
            TokenType = TokenType.PasswordReset,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        }, rawToken);
    }

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
}
