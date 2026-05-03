using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Auth.Logout;

public sealed class LogoutUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public LogoutUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        await _refreshTokenRepository.MarkAllAsRevokedAsync(userId, ct);
        await _auditLogService.LogAsync(userId, "LOGOUT", cancellationToken: ct);
        await _context.SaveChangesAsync(ct);
    }
}
