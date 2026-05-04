using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class Alternative : BaseEntity<int>
{
    private Alternative() { }

    internal static Alternative Create(string letter, string text, bool isCorrect, string explanation)
    {
        return new Alternative
        {
            Letter = letter,
            Text = text,
            IsCorrect = isCorrect,
            Explanation = explanation,
            CreatedAt = DateTime.UtcNow
        };
    }

    public int QuestionId { get; private set; }
    public string Letter { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public string Explanation { get; private set; } = string.Empty;

    public Question? Question { get; private set; }
}
