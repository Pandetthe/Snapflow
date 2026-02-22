using FluentAssertions;
using Snapflow.Infrastructure.Common;

namespace Snapflow.UnitTests.Infrastructure;

public sealed class LexoRankServiceTests
{
    private readonly LexoRankService _sut = new();

    [Fact]
    public void GenerateInitial_Should_ReturnValidRank_When_Empty()
    {
        var result = _sut.GenerateInitial();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(LexoRankService.Length);
    }

    [Theory]
    [InlineData("000000000001", "000000000003", "000000000002")]
    [InlineData(null, "000000000002", "000000000001")]
    public void TryGenerateBetween_Should_ReturnMidValue_When_ValidRange(string? left, string? right, string expected)
    {
        var success = _sut.TryGenerateBetween(left, right, out var newRank);

        success.Should().BeTrue();
        newRank.Should().Be(expected);
    }

    [Fact]
    public void GenerateBalanced_Should_ReturnEvenlySpacedRanks_When_CountRequested()
    {
        int count = 3;

        var results = _sut.GenerateBalanced(count);

        results.Should().HaveCount(count);
        string.CompareOrdinal(results[0], results[1]).Should().BeLessThan(0);
        string.CompareOrdinal(results[1], results[2]).Should().BeLessThan(0);
    }
}
