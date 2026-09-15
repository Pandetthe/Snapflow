using FluentAssertions;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class UserNameSuggestionTests
{
    [Theory]
    [InlineData("Jan Kowalski", null, "Jan.Kowalski")]
    [InlineData("Zoë Łęcka", null, "Zoe.Lecka")]
    [InlineData("  O'Brien  -  Smith ", null, "OBrien.Smith")]
    [InlineData(null, "jan.k+work@example.com", "jan.kwork")]
    [InlineData("李", "ab@example.com", "user")]
    public void From_Should_ReturnValidUserName_When_NameOrEmailGiven(string? name, string? email, string expected)
    {
        var result = UserNameSuggestion.From(name, email);

        result.Should().Be(expected);
    }

    [Fact]
    public void From_Should_TruncateToMaxLength_When_NameTooLong()
    {
        var result = UserNameSuggestion.From("Aleksandra Wiśniewska-Nowakowska", null);

        result.Length.Should().BeLessThanOrEqualTo(UserOptions.MaxUserNameLength);
        result.Should().Be("Aleksandra.Wisniewsk");
    }

    [Fact]
    public void WithNumber_Should_StayWithinMaxLength_When_SuggestionIsFull()
    {
        var result = UserNameSuggestion.WithNumber("Aleksandra.Wisniewsk", 12);

        result.Should().Be("Aleksandra.Wisniew12");
    }

    [Fact]
    public void WithNumber_Should_NotLeaveSeparatorBeforeNumber_When_TruncatedAtDot()
    {
        var result = UserNameSuggestion.WithNumber("Aleksandra.Wisniewsk", 123456789);

        result.Should().Be("Aleksandra123456789");
    }
}
