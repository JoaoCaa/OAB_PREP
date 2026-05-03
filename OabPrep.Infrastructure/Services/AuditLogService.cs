using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context) => _context = context;

    public async Task LogAsync(
        Guid userId,
        string action,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var entry = AuditLog.Create(userId, action, details);
        await _context.AuditLogs.AddAsync(entry, cancellationToken);
        // SaveChangesAsync é chamado pelo use case para garantir atomicidade
    }
}
