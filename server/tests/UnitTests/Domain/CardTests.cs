using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class CardTests
{
    private static IUser CreateUser(int id = 1, string userName = "john")
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.UserName.Returns(userName);
        return user;
    }

    [Fact]
    public void Create_Should_InitializeCard_And_RaiseEvent()
    {
        var boardId = 1;
        var swimlaneId = 2;
        var listId = 3;
        var title = "Test Card";
        var description = "Test Desc";
        var rank = "000000000001";
        var createdBy = CreateUser(7, "alice");
        var now = DateTimeOffset.UtcNow;

        var card = Card.Create(boardId, swimlaneId, listId, title, description, rank, createdBy, now, "conn-id");

        card.BoardId.Should().Be(boardId);
        card.SwimlaneId.Should().Be(swimlaneId);
        card.ListId.Should().Be(listId);
        card.Title.Should().Be(title);
        card.Description.Should().Be(description);
        card.Rank.Should().Be(rank);
        card.CreatedById.Should().Be(createdBy.Id);
        card.CreatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).Should().ContainSingle()
            .Which.Should().BeOfType<CardCreatedDomainEvent>()
            .Which.Should().Match<CardCreatedDomainEvent>(e =>
                e.CreatedAt == now && e.CreatedById == 7 && e.CreatedByUserName == "alice" && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var card = Card.Create(1, 2, 3, "Old", "Old Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        var newDesc = "New Desc";
        var updatedBy = CreateUser(2, "bob");
        var now = DateTimeOffset.UtcNow;

        card.Update(newTitle, newDesc, updatedBy, now, "new-conn");

        card.Title.Should().Be(newTitle);
        card.Description.Should().Be(newDesc);
        card.UpdatedById.Should().Be(2);
        card.UpdatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).OfType<CardUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardUpdatedDomainEvent>(e =>
                e.UpdatedById == 2 && e.UpdatedByUserName == "bob" && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var movedBy = CreateUser(5, "bob");
        var now = DateTimeOffset.UtcNow;

        card.Move(4, 6, "rank2", movedBy, now, "move-conn");

        card.ListId.Should().Be(4);
        card.SwimlaneId.Should().Be(6);
        card.Rank.Should().Be("rank2");
        card.UpdatedById.Should().Be(5);
        card.UpdatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).OfType<CardMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardMovedDomainEvent>(e =>
                e.ListId == 4 && e.Rank == "rank2" && e.MovedById == 5 && e.MovedByUserName == "bob" && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var deletedBy = CreateUser(3, "carol");
        var now = DateTimeOffset.UtcNow;

        card.SoftDelete(deletedBy, now);

        card.IsDeleted.Should().BeTrue();
        card.DeletedById.Should().Be(3);
        card.DeletedAt.Should().Be(now);
        card.DeletedByCascade.Should().BeFalse();

        card.DomainEvents.Select(e => e(card)).OfType<CardDeletedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardDeletedDomainEvent>(e => e.DeletedById == 3 && e.DeletedByUserName == "carol");
    }

    [Fact]
    public void AddTag_Should_PutTagOnCard_And_RaiseEventWithActor()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var tag = Tag.Create(1, "Bug", TagColors.Red, CreateUser(), DateTimeOffset.UtcNow);
        var addedBy = CreateUser(4, "dave");

        var added = card.AddTag(tag, addedBy, "tag-conn");

        added.Should().BeTrue();
        card.Tags.Should().ContainSingle().Which.Should().BeSameAs(tag);

        card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardTagAddedDomainEvent>(e =>
                e.AddedById == 4 && e.AddedByUserName == "dave" && e.ConnectionId == "tag-conn");
    }

    [Fact]
    public void AddTag_Should_DoNothing_When_TagIsAlreadyOnCard()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var tag = Tag.Create(1, "Bug", TagColors.Red, CreateUser(), DateTimeOffset.UtcNow);
        card.AddTag(tag, CreateUser());

        var added = card.AddTag(tag, CreateUser());

        added.Should().BeFalse();
        card.Tags.Should().ContainSingle();
        card.DomainEvents.Select(e => e(card)).OfType<CardTagAddedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RemoveTag_Should_TakeTagOffCard_And_RaiseEventWithActor()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var tag = Tag.Create(1, "Bug", TagColors.Red, CreateUser(), DateTimeOffset.UtcNow);
        card.AddTag(tag, CreateUser());
        var removedBy = CreateUser(5, "erin");

        var removed = card.RemoveTag(tag, removedBy, "untag-conn");

        removed.Should().BeTrue();
        card.Tags.Should().BeEmpty();

        card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<CardTagRemovedDomainEvent>(e =>
                e.RemovedById == 5 && e.RemovedByUserName == "erin" && e.ConnectionId == "untag-conn");
    }

    [Fact]
    public void RemoveTag_Should_DoNothing_When_TagIsNotOnCard()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var tag = Tag.Create(1, "Bug", TagColors.Red, CreateUser(), DateTimeOffset.UtcNow);

        var removed = card.RemoveTag(tag, CreateUser());

        removed.Should().BeFalse();
        card.DomainEvents.Select(e => e(card)).OfType<CardTagRemovedDomainEvent>().Should().BeEmpty();
    }
}
