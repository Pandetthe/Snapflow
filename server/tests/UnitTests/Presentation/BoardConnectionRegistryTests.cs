using Snapflow.Presentation.Hubs.Board;

namespace Snapflow.UnitTests.Presentation;

public sealed class BoardConnectionRegistryTests
{
    [Fact]
    public void GetConnectionIds_Should_BeEmpty_When_NothingWasAdded()
    {
        var registry = new BoardConnectionRegistry();

        Assert.Empty(registry.GetConnectionIds(1, 1));
    }

    [Fact]
    public void GetConnectionIds_Should_ReturnEveryConnectionOfTheUserOnTheBoard()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-b");

        Assert.Equal(new[] { "conn-a", "conn-b" }, registry.GetConnectionIds(1, 7).Order());
    }

    [Fact]
    public void GetConnectionIds_Should_KeepBoardsAndUsersApart()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(2, 7, "conn-b");
        registry.Add(1, 8, "conn-c");

        Assert.Equal(new[] { "conn-a" }, registry.GetConnectionIds(1, 7).Order());
        Assert.Equal(new[] { "conn-b" }, registry.GetConnectionIds(2, 7).Order());
        Assert.Equal(new[] { "conn-c" }, registry.GetConnectionIds(1, 8).Order());
    }

    [Fact]
    public void Add_Should_BeIdempotent_ForTheSameConnection()
    {
        var registry = new BoardConnectionRegistry();

        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-a");

        Assert.Single(registry.GetConnectionIds(1, 7));
    }

    [Fact]
    public void Remove_Should_LeaveTheUsersOtherConnections()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");
        registry.Add(1, 7, "conn-b");

        registry.Remove(1, 7, "conn-a");

        Assert.Equal(new[] { "conn-b" }, registry.GetConnectionIds(1, 7).Order());
    }

    [Fact]
    public void Remove_Should_DoNothing_When_TheConnectionIsNotKnown()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");

        registry.Remove(1, 7, "conn-missing");
        registry.Remove(9, 9, "conn-a");

        Assert.Equal(new[] { "conn-a" }, registry.GetConnectionIds(1, 7).Order());
    }

    [Fact]
    public void Add_Should_Work_When_TheUserReconnectsAfterTheirLastConnectionWentAway()
    {
        var registry = new BoardConnectionRegistry();
        registry.Add(1, 7, "conn-a");
        registry.Remove(1, 7, "conn-a");

        registry.Add(1, 7, "conn-b");

        Assert.Equal(new[] { "conn-b" }, registry.GetConnectionIds(1, 7).Order());
    }
}
