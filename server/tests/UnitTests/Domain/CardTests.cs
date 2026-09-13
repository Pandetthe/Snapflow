using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Cards;
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
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var card = Card.Create(1, 2, 3, "Old", "Old Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        var newDesc = "New Desc";
        var updaterId = 2;
        var now = DateTimeOffset.UtcNow;

        card.Update(newTitle, newDesc, updaterId, now, "new-conn");

        card.Title.Should().Be(newTitle);
        card.Description.Should().Be(newDesc);
        card.UpdatedById.Should().Be(updaterId);
        card.UpdatedAt.Should().Be(now);
        
        card.DomainEvents.Select(e => e(card)).Should().Contain(e => e is CardUpdatedDomainEvent);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", CreateUser(), DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        card.SoftDelete(deleterId, now);

        card.IsDeleted.Should().BeTrue();
        card.DeletedById.Should().Be(deleterId);
        card.DeletedAt.Should().Be(now);
        card.DeletedByCascade.Should().BeFalse();

        card.DomainEvents.Select(e => e(card)).Should().Contain(e => e is CardDeletedDomainEvent);
    }
}
