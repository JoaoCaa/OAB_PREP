using FluentValidation;
using OabPrep.Domain.Services;

namespace OabPrep.Application.UseCases.Auth.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .Must(PasswordPolicy.IsValid).WithMessage(
                "A senha deve ter no mínimo 8 caracteres, com letra maiúscula, minúscula, número e caractere especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória.")
            .Equal(x => x.NewPassword).WithMessage("As senhas não conferem.");
    }
}
