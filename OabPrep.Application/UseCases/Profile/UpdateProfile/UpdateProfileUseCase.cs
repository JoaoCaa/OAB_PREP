using FluentValidation;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.UpdateProfile;

public sealed class UpdateProfileUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;
    private readonly IValidator<UpdateProfileCommand> _validator;

    public UpdateProfileUseCase(
        IUserRepository userRepository,
        IApplicationDbContext context,
        IValidator<UpdateProfileCommand> validator)
    {
        _userRepository = userRepository;
        _context = context;
        _validator = validator;
    }

    public async Task ExecuteAsync(Guid userId, UpdateProfileCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var user = await _userRepository.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário", userId);

        user.UpdateProfile(command.Name);

        await _context.SaveChangesAsync(ct);
    }
}
