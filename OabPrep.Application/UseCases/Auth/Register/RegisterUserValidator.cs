using FluentValidation;
using OabPrep.Domain.Services;

namespace OabPrep.Application.UseCases.Auth.Register;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter pelo menos 3 caracteres.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.")
            .Must(PasswordPolicy.IsValid)
            .WithMessage("Senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("As senhas não conferem.");

        RuleFor(x => x.AcceptedTerms)
            .Must(accepted => accepted).WithMessage("Você deve aceitar os termos de uso.");
    }
}
