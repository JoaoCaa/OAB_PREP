using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Admin.Users;

public sealed class BlockUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLog;
    private readonly IApplicationDbContext _context;

    public BlockUserUseCase(
        IUserRepository userRepository,
        IAuditLogService auditLog,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _auditLog = auditLog;
        _context = context;
    }

    public async Task ExecuteAsync(Guid adminId, Guid targetId, CancellationToken ct = default)
    {
        if (adminId == targetId)
            throw new ArgumentException("Administrador não pode bloquear a própria conta.");

        var user = await _userRepository.FindByIdAsync(targetId, ct)
            ?? throw new NotFoundException("Usuário", targetId);

        if (!user.IsActive)
            throw new ConflictException("Usuário já está bloqueado.");

        user.Block();
        await _auditLog.LogAsync(adminId, "USER_BLOCKED", $"targetId={targetId}", ct);
        await _context.SaveChangesAsync(ct);
    }
}
