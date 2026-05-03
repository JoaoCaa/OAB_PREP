using FluentValidation;

namespace OabPrep.Application.UseCases.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
