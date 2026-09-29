using FluentAssertions;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
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

        swimlane.BoardId.Should().Be(1);
        swimlane.Title.Should().Be("Test Swimlane");
        swimlane.Height.Should().Be(100);
        swimlane.Rank.Should().Be("000000000001");
        swimlane.CreatedById.Should().Be(7);
        swimlane.CreatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneCreatedDomainEvent>(e => e.CreatedById == 7 && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var swimlane = Swimlane.Create(1, "Old", 100, "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        swimlane.Update("New Title", 200, 2, now, "new-conn");

        swimlane.Title.Should().Be("New Title");
        swimlane.Height.Should().Be(200);
        swimlane.UpdatedById.Should().Be(2);
        swimlane.UpdatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneUpdatedDomainEvent>(e => e.UpdatedById == 2 && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndHeightAreTheSame()
    {
        var swimlane = CreateSwimlane();
        swimlane.ClearDomainEvents();

        var changed = swimlane.Update("Title", 100, 2, DateTimeOffset.UtcNow, "new-conn");

        changed.Should().BeFalse();
        swimlane.UpdatedById.Should().BeNull();
        swimlane.UpdatedAt.Should().BeNull();
        swimlane.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Move_Should_UpdateRank_And_RaiseEventWithMover()
    {
        var swimlane = CreateSwimlane();
        var now = DateTimeOffset.UtcNow;

        swimlane.Move("rank2", 5, now, "move-conn");

        swimlane.Rank.Should().Be("rank2");
        swimlane.UpdatedById.Should().Be(5);
        swimlane.UpdatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneMovedDomainEvent>(e =>
                e.Rank == "rank2" && e.MovedById == 5 && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var swimlane = CreateSwimlane();
        var now = DateTimeOffset.UtcNow;

        swimlane.SoftDelete(3, now);

        swimlane.IsDeleted.Should().BeTrue();
        swimlane.DeletedById.Should().Be(3);
        swimlane.DeletedAt.Should().Be(now);
        swimlane.DeletedByCascade.Should().BeFalse();

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneDeletedDomainEvent>().Should().ContainSingle()
            .Which.DeletedById.Should().Be(3);
    }

    [Fact]
    public void SoftDelete_Should_CascadeToListsAndCards()
    {
        var swimlane = CreateSwimlane();
        var list = List.Create(1, swimlane.Id, "List", null, "rank", 1, DateTimeOffset.UtcNow);
        var card = Card.Create(1, swimlane.Id, list.Id, "Card", "", "rank", 1, DateTimeOffset.UtcNow);
        Loaded.Into(swimlane, "_lists", list);
        Loaded.Into(swimlane, "_cards", card);

        swimlane.SoftDelete(3, DateTimeOffset.UtcNow);

        list.IsDeleted.Should().BeTrue();
        list.DeletedByCascade.Should().BeTrue();
        card.IsDeleted.Should().BeTrue();
        card.DeletedByCascade.Should().BeTrue();
    }
}
