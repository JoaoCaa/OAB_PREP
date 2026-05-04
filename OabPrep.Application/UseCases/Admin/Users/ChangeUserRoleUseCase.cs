using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Constants;

namespace OabPrep.Application.UseCases.Admin.Users;

public sealed class ChangeUserRoleUseCase
{
    private static readonly IReadOnlySet<string> ValidRoles =
        new HashSet<string> { UserRoles.User, UserRoles.Admin };

    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLog;
    private readonly IApplicationDbContext _context;

    public ChangeUserRoleUseCase(
        IUserRepository userRepository,
        IAuditLogService auditLog,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _auditLog = auditLog;
        _context = context;
    }

    public async Task ExecuteAsync(
        Guid adminId,
        Guid targetId,
        ChangeUserRoleCommand command,
        CancellationToken ct = default)
    {
        if (!ValidRoles.Contains(command.Role))
            throw new ArgumentException($"Papel inválido. Valores aceitos: {string.Join(", ", ValidRoles)}.");

        var user = await _userRepository.FindByIdAsync(targetId, ct)
            ?? throw new NotFoundException("Usuário", targetId);

        if (user.Role == command.Role)
            throw new ConflictException($"Usuário já possui o papel '{command.Role}'.");

        var previousRole = user.Role;
        user.ChangeRole(command.Role);
        await _auditLog.LogAsync(adminId, "USER_ROLE_CHANGED",
            $"targetId={targetId};from={previousRole};to={command.Role}", ct);
        await _context.SaveChangesAsync(ct);
    }
}
