using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Questions.Create;

namespace OabPrep.Application.UseCases.Questions.GetById;

public sealed class GetQuestionByIdUseCase
{
    private readonly IQuestionRepository _repository;

    public GetQuestionByIdUseCase(IQuestionRepository repository) => _repository = repository;

    public async Task<QuestionDetailResponse> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var question = await _repository.FindByIdWithAlternativesAsync(id, ct)
            ?? throw new NotFoundException("Question", id);

        return CreateQuestionUseCase.MapToDetail(question, lawAreaName: null);
    }
}
