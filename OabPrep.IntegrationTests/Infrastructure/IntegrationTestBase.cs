using System.Net.Http.Json;
using Bogus;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Application.UseCases.Auth.Register;

namespace OabPrep.IntegrationTests.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTestBase : IClassFixture<OabPrepWebApplicationFactory>
{
    protected readonly OabPrepWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected static readonly Faker Faker = new("pt_BR");

    protected IntegrationTestBase(OabPrepWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<LoginResponse> RegisterAndLoginAsync(string? email = null, string? password = null)
    {
        var resolvedEmail = email ?? Faker.Internet.Email();
        var resolvedPassword = password ?? "Abc@1234";

        // Register
        var registerCmd = new RegisterUserCommand
        {
            Name = Faker.Name.FullName(),
            Email = resolvedEmail,
            Password = resolvedPassword,
            ConfirmPassword = resolvedPassword,
            AcceptedTerms = true
        };
        var regResp = await Client.PostAsJsonAsync("/api/v1/auth/register", registerCmd);
        regResp.EnsureSuccessStatusCode();

        // Confirm email via DB (bypass email flow in tests)
        await ConfirmEmailViaDbAsync(resolvedEmail);

        // Login
        var loginCmd = new LoginCommand(resolvedEmail, resolvedPassword);
        var loginResp = await Client.PostAsJsonAsync("/api/v1/auth/login", loginCmd);
        loginResp.EnsureSuccessStatusCode();
        return (await loginResp.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    private async Task ConfirmEmailViaDbAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OabPrep.Infrastructure.Persistence.ApplicationDbContext>();
        var user = db.Users.First(u => u.Email == email.ToLowerInvariant());
        user.ConfirmEmail();
        await db.SaveChangesAsync();
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollectionDefinition : ICollectionFixture<OabPrepWebApplicationFactory> { }
