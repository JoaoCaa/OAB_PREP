using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Auth.Register;

public sealed class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailTokenRepository _emailTokenRepository;
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IMapper _mapper;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IEmailTokenRepository emailTokenRepository,
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IBackgroundTaskQueue backgroundTaskQueue,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _emailTokenRepository = emailTokenRepository;
        _context = context;
        _passwordHasher = passwordHasher;
        _backgroundTaskQueue = backgroundTaskQueue;
        _mapper = mapper;
    }

    public async Task<RegisterUserResponse> ExecuteAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "E-mail já cadastrado.")
            });

        // BCrypt é CPU-intensivo; executa em thread-pool para não bloquear a thread da requisição
        var passwordHash = await Task.Run(() => _passwordHasher.Hash(command.Password), cancellationToken);

        // AutoMapper constrói o User via factory method
        var input = new CreateUserInput(command.Name, normalizedEmail, passwordHash);
        var user = _mapper.Map<User>(input);
        var emailToken = EmailToken.CreateConfirmation(user.Id);

        await _userRepository.AddAsync(user, cancellationToken);
        await _emailTokenRepository.AddAsync(emailToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Captura para closure antes do fire-and-forget
        var recipientEmail = user.Email;
        var recipientName = user.Name;
        var token = emailToken.Token;

        _backgroundTaskQueue.Enqueue(async (sp, ct) =>
        {
            var emailService = sp.GetRequiredService<IEmailService>();
            await emailService.SendConfirmationEmailAsync(recipientEmail, recipientName, token, ct);
        });

        return new RegisterUserResponse("Conta criada com sucesso. Verifique seu e-mail para confirmar sua conta.");
    }
}

// DTO interno — input tipado para AutoMapper mapear → User
internal sealed record CreateUserInput(string Name, string NormalizedEmail, string PasswordHash);
