using FluentAssertions;
using OabPrep.Application.UseCases.Auth.Login;

namespace OabPrep.UnitTests.Auth.Validators;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _sut.Validate(new LoginCommand("a@b.com", "pw"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "pw")]
    [InlineData("notanemail", "pw")]
    public void Validate_InvalidEmail_HasError(string email, string pw)
    {
        var result = _sut.Validate(new LoginCommand(email, pw));
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        var result = _sut.Validate(new LoginCommand("a@b.com", ""));
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
