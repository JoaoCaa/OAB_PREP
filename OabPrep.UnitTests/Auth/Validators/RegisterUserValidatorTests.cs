using FluentAssertions;
using OabPrep.Application.UseCases.Auth.Register;

namespace OabPrep.UnitTests.Auth.Validators;

public sealed class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _sut = new();

    private static RegisterUserCommand ValidCommand() => new()
    {
        Name = "João Silva",
        Email = "joao@email.com",
        Password = "Abc@1234",
        ConfirmPassword = "Abc@1234",
        AcceptedTerms = true
    };

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _sut.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_InvalidName_HasError(string name)
    {
        var result = _sut.Validate(ValidCommand() with { Name = name });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var result = _sut.Validate(ValidCommand() with { Email = email });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("short")]           // too short
    [InlineData("alllowercase1@")]  // no uppercase
    [InlineData("ALLUPPERCASE1@")]  // no lowercase
    [InlineData("NoSpecialChar1")]  // no special char
    public void Validate_WeakPassword_HasError(string password)
    {
        var result = _sut.Validate(ValidCommand() with { Password = password, ConfirmPassword = password });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordMismatch_HasError()
    {
        var result = _sut.Validate(ValidCommand() with { ConfirmPassword = "Different@1" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public void Validate_TermsNotAccepted_HasError()
    {
        var result = _sut.Validate(ValidCommand() with { AcceptedTerms = false });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AcceptedTerms");
    }
}
