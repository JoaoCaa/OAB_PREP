using FluentValidation;
using OabPrep.Application.UseCases.Questions;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Questions.Create;

public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.LawAreaId).GreaterThan(0);
        RuleFor(x => x.Statement).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.Year).InclusiveBetween(1994, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.ExamEdition).MaximumLength(50).When(x => x.ExamEdition is not null);
        RuleFor(x => x.Explanation).MaximumLength(5000).When(x => x.Explanation is not null);
        RuleFor(x => x.Difficulty)
            .Must(d => Enum.IsDefined(typeof(DifficultyLevel), d))
            .WithMessage("Dificuldade inválida. Use 1 (Fácil), 2 (Médio) ou 3 (Difícil).");

        RuleFor(x => x.Alternatives)
            .NotNull()
            .Must(a => a.Count == 5)
            .WithMessage("A questão deve ter exatamente 5 alternativas.");

        RuleFor(x => x.Alternatives)
            .Must(a => a.Count(x => x.IsCorrect) == 1)
            .WithMessage("A questão deve ter exatamente 1 alternativa correta.")
            .When(x => x.Alternatives?.Count == 5);

        RuleForEach(x => x.Alternatives).ChildRules(alt =>
        {
            alt.RuleFor(a => a.Text).NotEmpty().MaximumLength(1000);
            alt.RuleFor(a => a.Explanation).NotEmpty().MaximumLength(2000);
        });
    }
}
