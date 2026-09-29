using Snapflow.Domain.Lists;

namespace Snapflow.UnitTests.Domain;

public sealed class ListTests
{
    private static List CreateList() =>
        List.Create(1, 2, "Title", 300, "rank", 1, DateTimeOffset.UtcNow);

    [Fact]
    public void Create_Should_InitializeList_And_RaiseEventWithCreator()
    {
        var now = DateTimeOffset.UtcNow;

        var list = List.Create(1, 2, "Test List", 300, "000000000001", 7, now, "conn-id");

        Assert.Equal(1, list.BoardId);
        Assert.Equal(2, list.SwimlaneId);
        Assert.Equal("Test List", list.Title);
        Assert.Equal(300, list.Width);
        Assert.Equal("000000000001", list.Rank);
        Assert.Equal(7, list.CreatedById);
        Assert.Equal(now, list.CreatedAt);

        ListCreatedDomainEvent raised = Assert.Single(list.DomainEvents.Select(e => e(list)).OfType<ListCreatedDomainEvent>());
        Assert.True(raised.CreatedById == 7 && raised.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var list = List.Create(1, 2, "Old", 300, "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        list.Update("New Title", 400, 2, now, "new-conn");

        Assert.Equal("New Title", list.Title);
        Assert.Equal(400, list.Width);
        Assert.Equal(2, list.UpdatedById);
        Assert.Equal(now, list.UpdatedAt);

        ListUpdatedDomainEvent raised = Assert.Single(list.DomainEvents.Select(e => e(list)).OfType<ListUpdatedDomainEvent>());
        Assert.True(raised.UpdatedById == 2 && raised.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndWidthAreTheSame()
    {
        var list = CreateList();
        list.ClearDomainEvents();

        var changed = list.Update("Title", 300, 2, DateTimeOffset.UtcNow, "new-conn");

        Assert.False(changed);
        Assert.Null(list.UpdatedById);
        Assert.Null(list.UpdatedAt);
        Assert.Empty(list.DomainEvents);
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var list = CreateList();
        var now = DateTimeOffset.UtcNow;

        list.Move(3, "rank2", 5, now, "move-conn");

        Assert.Equal(3, list.SwimlaneId);
        Assert.Equal("rank2", list.Rank);
        Assert.Equal(5, list.UpdatedById);
        Assert.Equal(now, list.UpdatedAt);

        ListMovedDomainEvent raised = Assert.Single(list.DomainEvents.Select(e => e(list)).OfType<ListMovedDomainEvent>());
        Assert.True(raised.SwimlaneId == 3 && raised.Rank == "rank2" && raised.MovedById == 5 && raised.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var list = CreateList();
        var now = DateTimeOffset.UtcNow;

        list.SoftDelete(3, now);

        Assert.True(list.IsDeleted);
        Assert.Equal(3, list.DeletedById);
        Assert.Equal(now, list.DeletedAt);
        Assert.False(list.DeletedByCascade);

        Assert.Equal(3, Assert.Single(list.DomainEvents.Select(e => e(list)).OfType<ListDeletedDomainEvent>()).DeletedById);
    }

}
