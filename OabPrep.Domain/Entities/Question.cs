using OabPrep.Domain.Common;
using OabPrep.Domain.Enums;

namespace OabPrep.Domain.Entities;

public sealed class Question : BaseEntity<int>
{
    private static readonly string[] Letters = ["A", "B", "C", "D", "E"];

    private Question() { }

    private readonly List<Alternative> _alternatives = [];

    public static Question Create(
        int lawAreaId,
        string statement,
        int year,
        string? examEdition,
        string? explanation,
        string? legalRefs,
        DifficultyLevel difficulty,
        IReadOnlyList<AlternativeData> alternatives)
    {
        EnsureValidAlternatives(alternatives);

        var question = new Question
        {
            LawAreaId = lawAreaId,
            Statement = statement,
            Year = year,
            ExamEdition = examEdition,
            Explanation = explanation,
            LegalRefs = legalRefs,
            Difficulty = difficulty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < alternatives.Count; i++)
        {
            var a = alternatives[i];
            question._alternatives.Add(Alternative.Create(Letters[i], a.Text, a.IsCorrect, a.Explanation));
        }

        return question;
    }

    public int LawAreaId { get; private set; }
    public string Statement { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string? ExamEdition { get; private set; }
    public string? Explanation { get; private set; }
    public string? LegalRefs { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public bool IsActive { get; private set; } = true;

    public LawArea? LawArea { get; private set; }
    public IReadOnlyList<Alternative> Alternatives => _alternatives.AsReadOnly();

    public void Update(
        int lawAreaId,
        string statement,
        int year,
        string? examEdition,
        string? explanation,
        string? legalRefs,
        DifficultyLevel difficulty,
        IReadOnlyList<AlternativeData> alternatives)
    {
        EnsureValidAlternatives(alternatives);

        LawAreaId = lawAreaId;
        Statement = statement;
        Year = year;
        ExamEdition = examEdition;
        Explanation = explanation;
        LegalRefs = legalRefs;
        Difficulty = difficulty;
        UpdatedAt = DateTime.UtcNow;

        _alternatives.Clear();
        for (var i = 0; i < alternatives.Count; i++)
        {
            var a = alternatives[i];
            _alternatives.Add(Alternative.Create(Letters[i], a.Text, a.IsCorrect, a.Explanation));
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void EnsureValidAlternatives(IReadOnlyList<AlternativeData> alternatives)
    {
        if (alternatives.Count != 5)
            throw new ArgumentException("A questão deve ter exatamente 5 alternativas.");

        if (alternatives.Count(a => a.IsCorrect) != 1)
            throw new ArgumentException("A questão deve ter exatamente 1 alternativa correta.");

        if (alternatives.Any(a => string.IsNullOrWhiteSpace(a.Explanation)))
            throw new ArgumentException("Todas as alternativas devem ter uma explicação.");
    }
}

public record AlternativeData(string Text, bool IsCorrect, string Explanation);
