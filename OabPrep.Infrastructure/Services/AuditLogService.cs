using Microsoft.EntityFrameworkCore;
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

    public Task<DateTime?> GetLastActionDateAsync(
        Guid userId,
        string action,
        CancellationToken cancellationToken = default) =>
        _context.AuditLogs
            .Where(a => a.UserId == userId && a.Action == action)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => (DateTime?)a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
