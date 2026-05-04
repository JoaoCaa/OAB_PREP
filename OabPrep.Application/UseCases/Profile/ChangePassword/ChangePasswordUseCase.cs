using FluentValidation;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.ChangePassword;

public sealed class ChangePasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IApplicationDbContext _context;
    private readonly IValidator<ChangePasswordCommand> _validator;

    public ChangePasswordUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IApplicationDbContext context,
        IValidator<ChangePasswordCommand> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _context = context;
        _validator = validator;
    }

    public async Task ExecuteAsync(Guid userId, ChangePasswordCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var user = await _userRepository.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário", userId);

        if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Senha atual incorreta.");

        user.UpdatePassword(_passwordHasher.Hash(command.NewPassword));

        await _context.SaveChangesAsync(ct);
    }
}
