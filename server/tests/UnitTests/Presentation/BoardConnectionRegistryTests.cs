using FluentAssertions;
using Snapflow.Presentation.Hubs.Board;

namespace Snapflow.UnitTests.Presentation;

public sealed class BoardConnectionRegistryTests
{
    [Fact]
    public void GetConnectionIds_Should_BeEmpty_When_NothingWasAdded()
    {
        var registry = new BoardConnectionRegistry();

        registry.GetConnectionIds(1, 1).Should().BeEmpty();
    }

    [Fact]
    public void GetConnectionIds_Should_ReturnEveryConnectionOfTheUserOnTheBoard()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-b");

        registry.GetConnectionIds(1, 7).Should().BeEquivalentTo(["conn-a", "conn-b"]);
    }

    [Fact]
    public void GetConnectionIds_Should_KeepBoardsAndUsersApart()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(2, 7, "conn-b");
        registry.Add(1, 8, "conn-c");

        registry.GetConnectionIds(1, 7).Should().BeEquivalentTo(["conn-a"]);
        registry.GetConnectionIds(2, 7).Should().BeEquivalentTo(["conn-b"]);
        registry.GetConnectionIds(1, 8).Should().BeEquivalentTo(["conn-c"]);
    }

    [Fact]
    public void Add_Should_BeIdempotent_ForTheSameConnection()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-a");

        registry.GetConnectionIds(1, 7).Should().ContainSingle();
    }

    [Fact]
    public void Remove_Should_LeaveTheUsersOtherConnections()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-b");

        registry.Remove(1, 7, "conn-a");

        registry.GetConnectionIds(1, 7).Should().BeEquivalentTo(["conn-b"]);
    }

    [Fact]
    public void Remove_Should_DoNothing_When_TheConnectionIsNotKnown()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");

        registry.Remove(1, 7, "conn-missing");
        registry.Remove(9, 9, "conn-a");

        registry.GetConnectionIds(1, 7).Should().BeEquivalentTo(["conn-a"]);
    }

    [Fact]
    public void Add_Should_Work_When_TheUserReconnectsAfterTheirLastConnectionWentAway()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");
        registry.Remove(1, 7, "conn-a");

        registry.Add(1, 7, "conn-b");

        registry.GetConnectionIds(1, 7).Should().BeEquivalentTo(["conn-b"]);
    }
}
