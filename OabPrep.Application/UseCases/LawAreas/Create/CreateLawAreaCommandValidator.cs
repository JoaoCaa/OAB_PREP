using FluentValidation;

namespace OabPrep.Application.UseCases.LawAreas.Create;

public sealed class CreateLawAreaCommandValidator : AbstractValidator<CreateLawAreaCommand>
{
    public CreateLawAreaCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.IconUrl)
            .MaximumLength(300)
            .When(x => x.IconUrl is not null);
    }
}
