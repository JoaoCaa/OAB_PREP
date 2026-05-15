using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using OabPrep.IntegrationTests.Infrastructure;

namespace OabPrep.IntegrationTests.Sessions;

public sealed class SessionFlowTests : IntegrationTestBase
{
    public SessionFlowTests(OabPrepWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateSession_Unauthenticated_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/sessions",
            new { QuestionCount = 5, ExcludeAnswered = false });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSession_NoQuestionsInDb_Returns400()
    {
        var login = await RegisterAndLoginAsync();
        var authed = Factory.CreateAuthenticatedClient(login.AccessToken);

        var response = await authed.PostAsJsonAsync("/api/v1/sessions",
            new { QuestionCount = 5, ExcludeAnswered = false });

        // No questions seeded — ArgumentException → 400
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
