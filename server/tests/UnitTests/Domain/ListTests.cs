using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class ListTests
{
    [Fact]
    public void Create_Should_InitializeList_And_RaiseEvent()
    {
        var boardId = 1;
        var swimlaneId = 2;
        var title = "Test List";
        int? width = 300;
        var rank = "000000000001";
        var createdById = 1;
        var now = DateTimeOffset.UtcNow;

        var list = List.Create(boardId, swimlaneId, title, width, rank, createdById, now, "conn-id");

        list.BoardId.Should().Be(boardId);
        list.SwimlaneId.Should().Be(swimlaneId);
        list.Title.Should().Be(title);
        list.Width.Should().Be(width);
        list.Rank.Should().Be(rank);
        list.CreatedById.Should().Be(createdById);
        list.CreatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).Should().ContainSingle(e => e is ListCreatedDomainEvent);
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var list = List.Create(1, 2, "Old", 300, "rank", 1, DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        int? newWidth = 400;
        var updaterId = 2;
        var now = DateTimeOffset.UtcNow;

        list.Update(newTitle, newWidth, updaterId, now, "new-conn");

        list.Title.Should().Be(newTitle);
        list.Width.Should().Be(newWidth);
        list.UpdatedById.Should().Be(updaterId);
        list.UpdatedAt.Should().Be(now);
        
        list.DomainEvents.Select(e => e(list)).Should().Contain(e => e is ListUpdatedDomainEvent);
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var list = List.Create(1, 2, "Title", 300, "rank", 1, DateTimeOffset.UtcNow);
        var movedBy = Substitute.For<IUser>();
        movedBy.Id.Returns(5);
        movedBy.UserName.Returns("bob");
        var now = DateTimeOffset.UtcNow;

        list.Move(3, "rank2", movedBy, now, "move-conn");

        list.SwimlaneId.Should().Be(3);
        list.Rank.Should().Be("rank2");
        list.UpdatedById.Should().Be(5);
        list.UpdatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListMovedDomainEvent>(e =>
                e.SwimlaneId == 3 && e.Rank == "rank2" && e.MovedById == 5 && e.MovedByUserName == "bob" && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var list = List.Create(1, 2, "Title", 300, "rank", 1, DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        list.SoftDelete(deleterId, now);

        list.IsDeleted.Should().BeTrue();
        list.DeletedById.Should().Be(deleterId);
        list.DeletedAt.Should().Be(now);
        list.DeletedByCascade.Should().BeFalse();

        list.DomainEvents.Select(e => e(list)).Should().Contain(e => e is ListDeletedDomainEvent);
    }
}
