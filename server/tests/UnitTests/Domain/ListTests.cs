using FluentAssertions;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;

namespace Snapflow.UnitTests.Domain;

public sealed class ListTests
{
    private static List CreateList() =>
        List.Create(1, 2, "Title", 300, "rank", 1, DateTimeOffset.UtcNow);

    private static Card CreateCard(List list) =>
        Card.Create(1, list.SwimlaneId, list.Id, "Card", "", "rank", 1, DateTimeOffset.UtcNow);

    [Fact]
    public void Create_Should_InitializeList_And_RaiseEventWithCreator()
    {
        var now = DateTimeOffset.UtcNow;

        var list = List.Create(1, 2, "Test List", 300, "000000000001", 7, now, "conn-id");

        list.BoardId.Should().Be(1);
        list.SwimlaneId.Should().Be(2);
        list.Title.Should().Be("Test List");
        list.Width.Should().Be(300);
        list.Rank.Should().Be("000000000001");
        list.CreatedById.Should().Be(7);
        list.CreatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListCreatedDomainEvent>(e => e.CreatedById == 7 && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var list = List.Create(1, 2, "Old", 300, "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        list.Update("New Title", 400, 2, now, "new-conn");

        list.Title.Should().Be("New Title");
        list.Width.Should().Be(400);
        list.UpdatedById.Should().Be(2);
        list.UpdatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListUpdatedDomainEvent>(e => e.UpdatedById == 2 && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndWidthAreTheSame()
    {
        var list = CreateList();
        list.ClearDomainEvents();

        var changed = list.Update("Title", 300, 2, DateTimeOffset.UtcNow, "new-conn");

        changed.Should().BeFalse();
        list.UpdatedById.Should().BeNull();
        list.UpdatedAt.Should().BeNull();
        list.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var list = CreateList();
        var now = DateTimeOffset.UtcNow;

        list.Move(3, "rank2", 5, now, "move-conn");

        list.SwimlaneId.Should().Be(3);
        list.Rank.Should().Be("rank2");
        list.UpdatedById.Should().Be(5);
        list.UpdatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListMovedDomainEvent>(e =>
                e.SwimlaneId == 3 && e.Rank == "rank2" && e.MovedById == 5 && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void Move_Should_CarryCardsToNewSwimlane()
    {
        var list = CreateList();
        var card = CreateCard(list);
        Loaded.Into(list, "_cards", card);

        list.Move(3, "rank2", 1, DateTimeOffset.UtcNow);

        card.SwimlaneId.Should().Be(3);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var list = CreateList();
        var now = DateTimeOffset.UtcNow;

        list.SoftDelete(3, now);

        list.IsDeleted.Should().BeTrue();
        list.DeletedById.Should().Be(3);
        list.DeletedAt.Should().Be(now);
        list.DeletedByCascade.Should().BeFalse();

        list.DomainEvents.Select(e => e(list)).OfType<ListDeletedDomainEvent>().Should().ContainSingle()
            .Which.DeletedById.Should().Be(3);
    }

    [Fact]
    public void SoftDelete_Should_CascadeToCards_WithoutTheirOwnEvents()
    {
        var list = CreateList();
        var card = CreateCard(list);
        card.ClearDomainEvents();
        Loaded.Into(list, "_cards", card);
        var now = DateTimeOffset.UtcNow;

        list.SoftDelete(3, now);

        card.IsDeleted.Should().BeTrue();
        card.DeletedByCascade.Should().BeTrue();
        card.DeletedById.Should().Be(3);
        card.DeletedAt.Should().Be(now);
        card.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_Should_KeepCardsDeletedEarlier()
    {
        var list = CreateList();
        var card = CreateCard(list);
        var earlier = DateTimeOffset.UtcNow.AddDays(-1);
        card.SoftDelete(9, earlier);
        Loaded.Into(list, "_cards", card);

        list.SoftDelete(3, DateTimeOffset.UtcNow);

        card.DeletedById.Should().Be(9);
        card.DeletedAt.Should().Be(earlier);
        card.DeletedByCascade.Should().BeFalse();
    }
}
