using FluentValidation;

namespace OabPrep.Application.UseCases.Chat.SendSessionMessage;

public sealed class SendSessionChatMessageValidator : AbstractValidator<SendSessionChatMessageCommand>
{
    public SendSessionChatMessageValidator()
    {
        RuleFor(c => c.Message)
            .NotEmpty().WithMessage("A mensagem não pode ser vazia.")
            .MaximumLength(500).WithMessage("A mensagem não pode exceder 500 caracteres.");
    }
}
