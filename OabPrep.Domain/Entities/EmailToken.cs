using System.Security.Cryptography;
using OabPrep.Domain.Common;
using OabPrep.Domain.Enums;

namespace OabPrep.Domain.Entities;

public sealed class EmailToken : BaseEntity<int>
{
    private EmailToken() { }

    public static EmailToken CreateConfirmation(Guid userId)
    {
        return new EmailToken
        {
            UserId = userId,
            Token = GenerateToken(),
            TokenType = TokenType.EmailConfirm,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    private static string GenerateToken() =>
        Convert.ToHexString(SHA256.HashData(RandomNumberGenerator.GetBytes(32)));

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public TokenType TokenType { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public User? User { get; private set; }
}
