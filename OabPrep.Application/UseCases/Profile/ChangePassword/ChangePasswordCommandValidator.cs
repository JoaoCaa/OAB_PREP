using FluentValidation;
using OabPrep.Domain.Services;

namespace OabPrep.Application.UseCases.Profile.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Senha atual é obrigatória.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("Nova senha deve ter pelo menos 8 caracteres.")
            .Must(PasswordPolicy.IsValid)
            .WithMessage("Nova senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("As senhas não conferem.");
    }
}
