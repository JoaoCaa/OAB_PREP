using FluentValidation;

namespace OabPrep.Application.UseCases.Sessions.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.SelectedAlternativeId).GreaterThan(0);
        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TimeSpentSeconds.HasValue);
    }
}
