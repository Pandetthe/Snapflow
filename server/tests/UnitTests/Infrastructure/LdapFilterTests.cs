using FluentAssertions;
using Snapflow.Infrastructure.Auth.External;

namespace Snapflow.UnitTests.Infrastructure;

public sealed class LdapFilterTests
{
    [Theory]
    [InlineData("jan.kowalski", "jan.kowalski")]
    [InlineData("*", @"\2a")]
    [InlineData("x)(uid=*", @"x\29\28uid=\2a")]
    [InlineData(@"a\b", @"a\5cb")]
    [InlineData("a\0b", @"a\00b")]
    public void Escape_Should_EscapeFilterCharacters_When_Present(string value, string expected)
    {
        LdapFilter.Escape(value).Should().Be(expected);
    }
}
