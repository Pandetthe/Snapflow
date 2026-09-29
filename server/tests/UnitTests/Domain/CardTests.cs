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

        Assert.Equal(1, card.BoardId);
        Assert.Equal(2, card.SwimlaneId);
        Assert.Equal(3, card.ListId);
        Assert.Equal("Test Card", card.Title);
        Assert.Equal("Test Desc", card.Description);
        Assert.Equal("000000000001", card.Rank);
        Assert.Equal(7, card.CreatedById);
        Assert.Equal(now, card.CreatedAt);

        CardCreatedDomainEvent raised = Assert.IsType<CardCreatedDomainEvent>(Assert.Single(card.DomainEvents.Select(e => e(card))));
        Assert.True(raised.CreatedAt == now && raised.CreatedById == 7 && raised.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var card = Card.Create(1, 2, 3, "Old", "Old Desc", "rank", 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        card.Update("New Title", "New Desc", 2, now, "new-conn");

        Assert.Equal("New Title", card.Title);
        Assert.Equal("New Desc", card.Description);
        Assert.Equal(2, card.UpdatedById);
        Assert.Equal(now, card.UpdatedAt);

        CardUpdatedDomainEvent raised = Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardUpdatedDomainEvent>());
        Assert.True(raised.UpdatedById == 2 && raised.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndDescriptionAreTheSame()
    {
        var card = CreateCard();
        card.ClearDomainEvents();

        var changed = card.Update("Title", "Desc", 2, DateTimeOffset.UtcNow, "new-conn");

        Assert.False(changed);
        Assert.Null(card.UpdatedById);
        Assert.Null(card.UpdatedAt);
        Assert.Empty(card.DomainEvents);
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var card = CreateCard();
        var now = DateTimeOffset.UtcNow;

        card.Move(4, 6, "rank2", 5, now, "move-conn");

        Assert.Equal(4, card.ListId);
        Assert.Equal(6, card.SwimlaneId);
        Assert.Equal("rank2", card.Rank);
        Assert.Equal(5, card.UpdatedById);
        Assert.Equal(now, card.UpdatedAt);

        CardMovedDomainEvent raised = Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardMovedDomainEvent>());
        Assert.True(raised.ListId == 4 && raised.Rank == "rank2" && raised.MovedById == 5 && raised.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var card = CreateCard();
        var now = DateTimeOffset.UtcNow;

        card.SoftDelete(3, now);

        Assert.True(card.IsDeleted);
        Assert.Equal(3, card.DeletedById);
        Assert.Equal(now, card.DeletedAt);
        Assert.False(card.DeletedByCascade);

        Assert.Equal(3, Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardDeletedDomainEvent>()).DeletedById);
    }

    [Fact]
    public void AddTag_Should_PutTagOnCard_And_RaiseEventWithActor()
    {
        var (card, tag) = CreateCardWithBoardTag();

        var added = card.AddTag(tag, 4, "tag-conn");

        Assert.True(added.IsSuccess);
        Assert.Same(tag, Assert.Single(card.Tags));

        CardTagAddedDomainEvent raised = Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>());
        Assert.True(raised.AddedById == 4 && raised.ConnectionId == "tag-conn");
    }

    [Fact]
    public void AddTag_Should_Fail_When_TagIsAlreadyOnCard()
    {
        var (card, tag) = CreateCardWithBoardTag();
        card.AddTag(tag, 1);

        var added = card.AddTag(tag, 1);

        Assert.Equal("Tags.AlreadyOnCard", added.Error.Code);
        Assert.Single(card.Tags);
        Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>());
    }

    [Fact]
    public void AddTag_Should_Fail_When_TagBelongsToAnotherBoard()
    {
        var (_, tag) = CreateCardWithBoardTag();
        var card = CreateCard(boardId: tag.BoardId + 1);

        var added = card.AddTag(tag, 1);

        Assert.Equal("Tags.NotFound", added.Error.Code);
        Assert.Empty(card.Tags);
    }

    [Fact]
    public void RemoveTag_Should_TakeTagOffCard_And_RaiseEventWithActor()
    {
        var (card, tag) = CreateCardWithBoardTag();
        card.AddTag(tag, 1);

        var removed = card.RemoveTag(tag, 5, "untag-conn");

        Assert.True(removed.IsSuccess);
        Assert.Empty(card.Tags);

        CardTagRemovedDomainEvent raised = Assert.Single(card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>());
        Assert.True(raised.RemovedById == 5 && raised.ConnectionId == "untag-conn");
    }

    [Fact]
    public void RemoveTag_Should_Fail_When_TagIsNotOnCard()
    {
        var (card, tag) = CreateCardWithBoardTag();

        var removed = card.RemoveTag(tag, 1);

        Assert.Equal("Tags.NotOnCard", removed.Error.Code);
        Assert.Empty(card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>());
    }
}
