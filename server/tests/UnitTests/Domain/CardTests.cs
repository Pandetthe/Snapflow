using FluentAssertions;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Tags;

namespace Snapflow.UnitTests.Domain;

public sealed class CardTests
{
    private static Board CreateBoard() =>
        Board.Create("Board", "", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);

    private static Card CreateCard(int boardId = 1) =>
        Card.Create(boardId, 2, 3, "Title", "Desc", "rank", 1, DateTimeOffset.UtcNow);

    private static (Card Card, Tag Tag) CreateCardWithBoardTag()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow).Value;
        return (CreateCard(board.Id), tag);
    }

    [Fact]
    public void Create_Should_InitializeCard_And_RaiseEvent()
    {
        var now = DateTimeOffset.UtcNow;

        var card = Card.Create(1, 2, 3, "Test Card", "Test Desc", "000000000001", 7, now, "conn-id");

        card.BoardId.Should().Be(1);
        card.SwimlaneId.Should().Be(2);
        card.ListId.Should().Be(3);
        card.Title.Should().Be("Test Card");
        card.Description.Should().Be("Test Desc");
        card.Rank.Should().Be("000000000001");
        card.CreatedById.Should().Be(7);
        card.CreatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).Should().ContainSingle()
            .Which.Should().BeOfType<CardCreatedDomainEvent>()
            .Which.Should().Match<CardCreatedDomainEvent>(e =>
                e.CreatedAt == now && e.CreatedById == 7 && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var card = Card.Create(1, 2, 3, "Old", "Old Desc", "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        card.Update("New Title", "New Desc", 2, now, "new-conn");

        card.Title.Should().Be("New Title");
        card.Description.Should().Be("New Desc");
        card.UpdatedById.Should().Be(2);
        card.UpdatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).OfType<CardUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardUpdatedDomainEvent>(e => e.UpdatedById == 2 && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndDescriptionAreTheSame()
    {
        var card = CreateCard();
        card.ClearDomainEvents();

        var changed = card.Update("Title", "Desc", 2, DateTimeOffset.UtcNow, "new-conn");

        changed.Should().BeFalse();
        card.UpdatedById.Should().BeNull();
        card.UpdatedAt.Should().BeNull();
        card.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var card = CreateCard();
        var now = DateTimeOffset.UtcNow;

        card.Move(4, 6, "rank2", 5, now, "move-conn");

        card.ListId.Should().Be(4);
        card.SwimlaneId.Should().Be(6);
        card.Rank.Should().Be("rank2");
        card.UpdatedById.Should().Be(5);
        card.UpdatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).OfType<CardMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardMovedDomainEvent>(e =>
                e.ListId == 4 && e.Rank == "rank2" && e.MovedById == 5 && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var card = CreateCard();
        var now = DateTimeOffset.UtcNow;

        card.SoftDelete(3, now);

        card.IsDeleted.Should().BeTrue();
        card.DeletedById.Should().Be(3);
        card.DeletedAt.Should().Be(now);
        card.DeletedByCascade.Should().BeFalse();

        card.DomainEvents.Select(e => e(card)).OfType<CardDeletedDomainEvent>().Should().ContainSingle()
            .Which.DeletedById.Should().Be(3);
    }

    [Fact]
    public void AddTag_Should_PutTagOnCard_And_RaiseEventWithActor()
    {
        var (card, tag) = CreateCardWithBoardTag();

        var added = card.AddTag(tag, 4, "tag-conn");

        added.IsSuccess.Should().BeTrue();
        card.Tags.Should().ContainSingle().Which.Should().BeSameAs(tag);

        card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardTagAddedDomainEvent>(e => e.AddedById == 4 && e.ConnectionId == "tag-conn");
    }

    [Fact]
    public void AddTag_Should_Fail_When_TagIsAlreadyOnCard()
    {
        var (card, tag) = CreateCardWithBoardTag();
        card.AddTag(tag, 1);

        var added = card.AddTag(tag, 1);

        added.Error.Code.Should().Be("Tags.AlreadyOnCard");
        card.Tags.Should().ContainSingle();
        card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void AddTag_Should_Fail_When_TagBelongsToAnotherBoard()
    {
        var (_, tag) = CreateCardWithBoardTag();
        var card = CreateCard(boardId: tag.BoardId + 1);

        var added = card.AddTag(tag, 1);

        added.Error.Code.Should().Be("Tags.NotFound");
        card.Tags.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_Should_TakeTagOffCard_And_RaiseEventWithActor()
    {
        var (card, tag) = CreateCardWithBoardTag();
        card.AddTag(tag, 1);

        var removed = card.RemoveTag(tag, 5, "untag-conn");

        removed.IsSuccess.Should().BeTrue();
        card.Tags.Should().BeEmpty();

        card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardTagRemovedDomainEvent>(e => e.RemovedById == 5 && e.ConnectionId == "untag-conn");
    }

    [Fact]
    public void RemoveTag_Should_Fail_When_TagIsNotOnCard()
    {
        var (card, tag) = CreateCardWithBoardTag();

        var removed = card.RemoveTag(tag, 1);

        removed.Error.Code.Should().Be("Tags.NotOnCard");
        card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>().Should().BeEmpty();
    }
}
