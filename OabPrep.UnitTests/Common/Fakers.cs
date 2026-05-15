using Bogus;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.UnitTests.Common;

internal static class Fakers
{
    private static readonly Faker F = new("pt_BR");

    public static User ActiveUser(string? email = null) =>
        User.Create(
            F.Name.FullName(),
            email ?? F.Internet.Email(),
            "$2a$12$dummyhashfortest0000000000000000000000000000000000000000");

    public static User ConfirmedUser(string? email = null)
    {
        var u = ActiveUser(email);
        u.ConfirmEmail();
        return u;
    }

    public static Question ValidQuestion(int lawAreaId = 1)
    {
        var alts = Enumerable.Range(0, 5).Select(i => new AlternativeData(
            F.Lorem.Sentence(),
            i == 0,
            F.Lorem.Sentence())).ToList();
        return Question.Create(lawAreaId, F.Lorem.Paragraph(), 2024, "1ª Fase",
            F.Lorem.Sentence(), null, DifficultyLevel.Medium, alts);
    }

    public static Session SessionFor(Guid userId, IEnumerable<int> questionIds) =>
        Session.Create(userId, questionIds);
}
