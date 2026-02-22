using FluentAssertions;
using Snapflow.Domain.Cards;

namespace Snapflow.UnitTests.Domain;

public sealed class CardTests
{
    [Fact]
    public void Create_Should_InitializeCard_And_RaiseEvent()
    {
        var boardId = 1;
        var swimlaneId = 2;
        var listId = 3;
        var title = "Test Card";
        var description = "Test Desc";
        var rank = "000000000001";
        var createdById = 1;
        var now = DateTimeOffset.UtcNow;

        var card = Card.Create(boardId, swimlaneId, listId, title, description, rank, createdById, now, "conn-id");

        card.BoardId.Should().Be(boardId);
        card.SwimlaneId.Should().Be(swimlaneId);
        card.ListId.Should().Be(listId);
        card.Title.Should().Be(title);
        card.Description.Should().Be(description);
        card.Rank.Should().Be(rank);
        card.CreatedById.Should().Be(createdById);
        card.CreatedAt.Should().Be(now);

        card.DomainEvents.Select(e => e(card)).Should().ContainSingle(e => e is CardCreatedDomainEvent);
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var card = Card.Create(1, 2, 3, "Old", "Old Desc", "rank", 1, DateTimeOffset.UtcNow);
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
        var card = Card.Create(1, 2, 3, "Title", "Desc", "rank", 1, DateTimeOffset.UtcNow);
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
