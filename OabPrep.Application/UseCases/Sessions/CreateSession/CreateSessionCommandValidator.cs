using FluentValidation;

namespace OabPrep.Application.UseCases.Sessions.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(5, 50)
            .WithMessage("O número de questões deve ser entre 5 e 50.");
    }
}
