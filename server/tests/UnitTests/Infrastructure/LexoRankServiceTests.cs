using Snapflow.Infrastructure.Common;

namespace Snapflow.UnitTests.Infrastructure;

public sealed class LexoRankServiceTests
{
    private readonly LexoRankService _sut = new();

    [Fact]
    public void GenerateInitial_Should_ReturnValidRank_When_Empty()
    {
        var result = _sut.GenerateInitial();

        Assert.False(string.IsNullOrEmpty(result));
        Assert.Equal(LexoRankService.Length, result.Length);
    }

    [Theory]
    [InlineData("000000000001", "000000000003", "000000000002")]
    [InlineData(null, "000000000002", "000000000001")]
    public void TryGenerateBetween_Should_ReturnMidValue_When_ValidRange(string? left, string? right, string expected)
    {
        var success = _sut.TryGenerateBetween(left, right, out var newRank);

        Assert.True(success);
        Assert.Equal(expected, newRank);
    }

    [Fact]
    public void TryGenerateBalanced_Should_ReturnEvenlySpacedRanks_When_CountRequested()
    {
        int count = 3;

        var success = _sut.TryGenerateBalanced(count, out var results);

        Assert.True(success);
        Assert.Equal(count, results!.Count);
        Assert.True(string.CompareOrdinal(results![0], results[1]) < 0);
        Assert.True(string.CompareOrdinal(results[1], results[2]) < 0);
    }

    [Fact]
    public void TryGenerateBalancedBetween_Should_KeepRanksInsideBounds()
    {
        var success = _sut.TryGenerateBalancedBetween(3, "000000000010", "000000000100", out var results);

        Assert.True(success);
        Assert.Equal(3, results!.Count);
        Assert.Equal(results.Order(StringComparer.Ordinal), results);
        Assert.All(results, rank => Assert.True(
            string.CompareOrdinal(rank, "000000000010") > 0 && string.CompareOrdinal(rank, "000000000100") < 0));
    }

    [Theory]
    [InlineData("000000000001", "000000000003")]
    [InlineData("000000000003", "000000000001")]
    public void TryGenerateBalancedBetween_Should_Fail_When_ThereIsNoRoom(string left, string right)
    {
        var success = _sut.TryGenerateBalancedBetween(3, left, right, out var results);

        Assert.False(success);
        Assert.Null(results);
    }
}
