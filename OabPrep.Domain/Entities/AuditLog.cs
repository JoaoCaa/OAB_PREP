namespace OabPrep.Domain.Entities;

public sealed class AuditLog
{
    private AuditLog() { }

    public static AuditLog Create(Guid userId, string action, string? details = null) => new()
    {
        UserId = userId,
        Action = action,
        Details = details,
        CreatedAt = DateTime.UtcNow
    };

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
