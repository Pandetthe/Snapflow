using Snapflow.Domain.Swimlanes;

namespace Snapflow.UnitTests.Domain;

public sealed class SwimlaneTests
{
    private static Swimlane CreateSwimlane() =>
        Swimlane.Create(1, "Title", 100, "rank", 1, DateTimeOffset.UtcNow);

    [Fact]
    public void Create_Should_InitializeSwimlane_And_RaiseEventWithCreator()
    {
        var now = DateTimeOffset.UtcNow;

        var swimlane = Swimlane.Create(1, "Test Swimlane", 100, "000000000001", 7, now, "conn-id");

        Assert.Equal(1, swimlane.BoardId);
        Assert.Equal("Test Swimlane", swimlane.Title);
        Assert.Equal(100, swimlane.Height);
        Assert.Equal("000000000001", swimlane.Rank);
        Assert.Equal(7, swimlane.CreatedById);
        Assert.Equal(now, swimlane.CreatedAt);

        SwimlaneCreatedDomainEvent raised = Assert.Single(swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneCreatedDomainEvent>());
        Assert.True(raised.CreatedById == 7 && raised.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var swimlane = Swimlane.Create(1, "Old", 100, "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        swimlane.Update("New Title", 200, 2, now, "new-conn");

        Assert.Equal("New Title", swimlane.Title);
        Assert.Equal(200, swimlane.Height);
        Assert.Equal(2, swimlane.UpdatedById);
        Assert.Equal(now, swimlane.UpdatedAt);

        SwimlaneUpdatedDomainEvent raised = Assert.Single(swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneUpdatedDomainEvent>());
        Assert.True(raised.UpdatedById == 2 && raised.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndHeightAreTheSame()
    {
        var swimlane = CreateSwimlane();
        swimlane.ClearDomainEvents();

        var changed = swimlane.Update("Title", 100, 2, DateTimeOffset.UtcNow, "new-conn");

        Assert.False(changed);
        Assert.Null(swimlane.UpdatedById);
        Assert.Null(swimlane.UpdatedAt);
        Assert.Empty(swimlane.DomainEvents);
    }

    [Fact]
    public void Move_Should_UpdateRank_And_RaiseEventWithMover()
    {
        var swimlane = CreateSwimlane();
        var now = DateTimeOffset.UtcNow;

        swimlane.Move("rank2", 5, now, "move-conn");

        Assert.Equal("rank2", swimlane.Rank);
        Assert.Equal(5, swimlane.UpdatedById);
        Assert.Equal(now, swimlane.UpdatedAt);

        SwimlaneMovedDomainEvent raised = Assert.Single(swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneMovedDomainEvent>());
        Assert.True(raised.Rank == "rank2" && raised.MovedById == 5 && raised.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var swimlane = CreateSwimlane();
        var now = DateTimeOffset.UtcNow;

        swimlane.SoftDelete(3, now);

        Assert.True(swimlane.IsDeleted);
        Assert.Equal(3, swimlane.DeletedById);
        Assert.Equal(now, swimlane.DeletedAt);
        Assert.False(swimlane.DeletedByCascade);

        Assert.Equal(3, Assert.Single(swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneDeletedDomainEvent>()).DeletedById);
    }

}
