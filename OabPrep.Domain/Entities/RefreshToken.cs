using System.Security.Cryptography;
using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class RefreshToken : BaseEntity<int>
{
    private RefreshToken() { }

    public static (RefreshToken Entity, string RawToken) Create(Guid userId, DateTime expiresAt)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(rawBytes);
        var storedToken = Convert.ToHexString(SHA256.HashData(rawBytes));

        return (new RefreshToken
        {
            UserId = userId,
            Token = storedToken,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        }, rawToken);
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public User? User { get; private set; }

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
