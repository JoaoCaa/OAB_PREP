using FluentValidation;

namespace OabPrep.Application.UseCases.Auth.ForgotPassword;

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.");
    }
}
