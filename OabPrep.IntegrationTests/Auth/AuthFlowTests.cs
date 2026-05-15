using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.IntegrationTests.Infrastructure;

namespace OabPrep.IntegrationTests.Auth;

public sealed class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(OabPrepWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidUser_Returns201()
    {
        var cmd = new RegisterUserCommand
        {
            Name = "Test User",
            Email = Faker.Internet.Email(),
            Password = "Abc@1234",
            ConfirmPassword = "Abc@1234",
            AcceptedTerms = true
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = Faker.Internet.Email();
        var cmd = new RegisterUserCommand
        {
            Name = "User", Email = email,
            Password = "Abc@1234", ConfirmPassword = "Abc@1234", AcceptedTerms = true
        };

        await Client.PostAsJsonAsync("/api/v1/auth/register", cmd);
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithoutEmailConfirmation_Returns401()
    {
        var email = Faker.Internet.Email();
        var cmd = new RegisterUserCommand
        {
            Name = "User", Email = email,
            Password = "Abc@1234", ConfirmPassword = "Abc@1234", AcceptedTerms = true
        };
        await Client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        var loginResp = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCommand(email, "Abc@1234"));

        loginResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullAuthFlow_RegisterConfirmLogin_ReturnsJwt()
    {
        var loginResponse = await RegisterAndLoginAsync();

        loginResponse.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var email = Faker.Internet.Email();
        await RegisterAndLoginAsync(email);

        var loginResp = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginCommand(email, "WrongPass@1"));

        loginResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
