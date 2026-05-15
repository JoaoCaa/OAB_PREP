using FluentAssertions;
using OabPrep.Application.UseCases.Chat.SendSessionMessage;

namespace OabPrep.UnitTests.Chat.Validators;

public sealed class SendSessionChatMessageValidatorTests
{
    private readonly SendSessionChatMessageValidator _sut = new();

    [Fact]
    public void Validate_ValidMessage_IsValid()
    {
        var result = _sut.Validate(new SendSessionChatMessageCommand("Explique o artigo."));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyMessage_HasError()
    {
        var result = _sut.Validate(new SendSessionChatMessageCommand(""));
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Fact]
    public void Validate_MessageOver500Chars_HasError()
    {
        var result = _sut.Validate(new SendSessionChatMessageCommand(new string('a', 501)));
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Fact]
    public void Validate_MessageExactly500Chars_IsValid()
    {
        var result = _sut.Validate(new SendSessionChatMessageCommand(new string('a', 500)));
        result.IsValid.Should().BeTrue();
    }
}
