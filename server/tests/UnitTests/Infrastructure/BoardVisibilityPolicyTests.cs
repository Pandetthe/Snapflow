using Microsoft.Extensions.Options;
using Snapflow.Domain.Boards;
using Snapflow.Infrastructure.Boards;

namespace Snapflow.UnitTests.Infrastructure;

public sealed class BoardVisibilityPolicyTests
{
    private static BoardVisibilityPolicy Create(params BoardVisibility[]? allowed) =>
        new(Options.Create(new BoardVisibilityOptions { AllowedVisibilities = allowed }));

    [Fact]
    public void AllowedVisibilities_Should_AllowEverything_When_NotConfigured()
    {
        var policy = Create(null);

        Assert.Equal([BoardVisibility.Private, BoardVisibility.Unlisted, BoardVisibility.Public], policy.AllowedVisibilities);
    }

    [Fact]
    public void AllowedVisibilities_Should_AlwaysContainPrivate()
    {
        var policy = Create(BoardVisibility.Public);

        Assert.Equal([BoardVisibility.Private, BoardVisibility.Public], policy.AllowedVisibilities);
    }

    [Theory]
    [InlineData(BoardVisibility.Unlisted, false)]
    [InlineData(BoardVisibility.Unlisted, true)]
    [InlineData(BoardVisibility.Public, false)]
    [InlineData(BoardVisibility.Public, true)]
    public void CanNonMemberView_Should_AllowUnlistedAndPublicBoards(BoardVisibility visibility, bool isAuthenticated)
    {
        var policy = Create(null);

        Assert.True(policy.CanNonMemberView(visibility, isAuthenticated));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanNonMemberView_Should_DenyPrivateBoard(bool isAuthenticated)
    {
        var policy = Create(null);

        Assert.False(policy.CanNonMemberView(BoardVisibility.Private, isAuthenticated));
    }

    [Theory]
    [InlineData(BoardVisibility.Private, false)]
    [InlineData(BoardVisibility.Unlisted, false)]
    [InlineData(BoardVisibility.Public, true)]
    public void IsListed_Should_ListOnlyPublicBoards(BoardVisibility visibility, bool expected)
    {
        var policy = Create(null);

        Assert.Equal(expected, policy.IsListed(visibility));
    }

    [Fact]
    public void Policy_Should_TreatPublicBoardAsUnlisted_When_OnlyPublicIsNoLongerAllowed()
    {
        var policy = Create(BoardVisibility.Private, BoardVisibility.Unlisted);

        Assert.False(policy.IsAllowed(BoardVisibility.Public));
        Assert.True(policy.CanNonMemberView(BoardVisibility.Public, isAuthenticated: false));
        Assert.False(policy.IsListed(BoardVisibility.Public));
    }

    [Fact]
    public void Policy_Should_TreatSharedBoardsAsPrivate_When_OnlyPrivateIsAllowed()
    {
        var policy = Create(BoardVisibility.Private);

        Assert.False(policy.CanNonMemberView(BoardVisibility.Unlisted, isAuthenticated: true));
        Assert.False(policy.CanNonMemberView(BoardVisibility.Public, isAuthenticated: true));
        Assert.False(policy.IsListed(BoardVisibility.Public));
    }
}
