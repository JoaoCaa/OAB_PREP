using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;
using OabPrep.Infrastructure.Persistence;
using OabPrep.IntegrationTests.Infrastructure;

namespace OabPrep.IntegrationTests.Chat;

public sealed class ChatLimitTests : IntegrationTestBase
{
    public ChatLimitTests(OabPrepWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task SendMessage_After20Messages_Returns429()
    {
        var login = await RegisterAndLoginAsync();
        var authed = Factory.CreateAuthenticatedClient(login.AccessToken);

        // Seed question + session via DB
        var (sessionId, questionId) = await SeedSessionAsync(login.UserId);

        // Send 20 messages (LLM is stubbed)
        for (var i = 0; i < 20; i++)
        {
            var r = await authed.PostAsJsonAsync(
                $"/api/v1/sessions/{sessionId}/questions/{questionId}/chat/messages",
                new { message = $"Pergunta {i}" });
            r.EnsureSuccessStatusCode();
        }

        // 21st message should return 429
        var limitResp = await authed.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/questions/{questionId}/chat/messages",
            new { message = "Mensagem extra" });

        limitResp.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private async Task<(int sessionId, int questionId)> SeedSessionAsync(Guid userId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Insert LawArea directly with raw SQL since the entity may have constraints
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO LawAreas (Name, IsActive, CreatedAt) VALUES ('Dir. Civil', 1, GETUTCDATE())");

        var area = await db.Set<LawArea>().FirstAsync();

        var alts = Enumerable.Range(0, 5).Select((_, i) =>
            new AlternativeData($"Alt {i}", i == 0, $"Exp {i}")).ToList();

        var q = Question.Create(area.Id, "Enunciado teste", 2024, null, "Explicação", null,
            DifficultyLevel.Medium, alts);
        db.Set<Question>().Add(q);
        await db.SaveChangesAsync();

        var session = Session.Create(userId, [q.Id]);
        db.Set<Session>().Add(session);
        await db.SaveChangesAsync();

        return (session.Id, q.Id);
    }
}
