using FluentAssertions;
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

        policy.AllowedVisibilities.Should().Equal(BoardVisibility.Private, BoardVisibility.Unlisted, BoardVisibility.Public);
    }

    [Fact]
    public void AllowedVisibilities_Should_AlwaysContainPrivate()
    {
        var policy = Create(BoardVisibility.Public);

        policy.AllowedVisibilities.Should().Equal(BoardVisibility.Private, BoardVisibility.Public);
    }

    [Theory]
    [InlineData(BoardVisibility.Unlisted, false)]
    [InlineData(BoardVisibility.Unlisted, true)]
    [InlineData(BoardVisibility.Public, false)]
    [InlineData(BoardVisibility.Public, true)]
    public void CanNonMemberView_Should_AllowUnlistedAndPublicBoards(BoardVisibility visibility, bool isAuthenticated)
    {
        var policy = Create(null);

        policy.CanNonMemberView(visibility, isAuthenticated).Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanNonMemberView_Should_DenyPrivateBoard(bool isAuthenticated)
    {
        var policy = Create(null);

        policy.CanNonMemberView(BoardVisibility.Private, isAuthenticated).Should().BeFalse();
    }

    [Theory]
    [InlineData(BoardVisibility.Private, false)]
    [InlineData(BoardVisibility.Unlisted, false)]
    [InlineData(BoardVisibility.Public, true)]
    public void IsListed_Should_ListOnlyPublicBoards(BoardVisibility visibility, bool expected)
    {
        var policy = Create(null);

        policy.IsListed(visibility).Should().Be(expected);
    }

    [Fact]
    public void Policy_Should_TreatPublicBoardAsUnlisted_When_OnlyPublicIsNoLongerAllowed()
    {
        var policy = Create(BoardVisibility.Private, BoardVisibility.Unlisted);

        policy.IsAllowed(BoardVisibility.Public).Should().BeFalse();
        policy.CanNonMemberView(BoardVisibility.Public, isAuthenticated: false).Should().BeTrue();
        policy.IsListed(BoardVisibility.Public).Should().BeFalse();
    }

    [Fact]
    public void Policy_Should_TreatSharedBoardsAsPrivate_When_OnlyPrivateIsAllowed()
    {
        var policy = Create(BoardVisibility.Private);

        policy.CanNonMemberView(BoardVisibility.Unlisted, isAuthenticated: true).Should().BeFalse();
        policy.CanNonMemberView(BoardVisibility.Public, isAuthenticated: true).Should().BeFalse();
        policy.IsListed(BoardVisibility.Public).Should().BeFalse();
    }
}
