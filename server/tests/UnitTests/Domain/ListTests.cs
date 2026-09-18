using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class ListTests
{
    private static IUser CreateUser(int id = 1, string userName = "john")
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.UserName.Returns(userName);
        return user;
    }

    [Fact]
    public void Create_Should_InitializeList_And_RaiseEventWithCreator()
    {
        var boardId = 1;
        var swimlaneId = 2;
        var title = "Test List";
        int? width = 300;
        var rank = "000000000001";
        var createdBy = CreateUser(7, "alice");
        var now = DateTimeOffset.UtcNow;

        var list = List.Create(boardId, swimlaneId, title, width, rank, createdBy, now, "conn-id");

        list.BoardId.Should().Be(boardId);
        list.SwimlaneId.Should().Be(swimlaneId);
        list.Title.Should().Be(title);
        list.Width.Should().Be(width);
        list.Rank.Should().Be(rank);
        list.CreatedById.Should().Be(7);
        list.CreatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListCreatedDomainEvent>(e =>
                e.CreatedById == 7 && e.CreatedByUserName == "alice" && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var list = List.Create(1, 2, "Old", 300, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        int? newWidth = 400;
        var updatedBy = CreateUser(2, "bob");
        var now = DateTimeOffset.UtcNow;

        list.Update(newTitle, newWidth, updatedBy, now, "new-conn");

        list.Title.Should().Be(newTitle);
        list.Width.Should().Be(newWidth);
        list.UpdatedById.Should().Be(2);
        list.UpdatedAt.Should().Be(now);

        list.DomainEvents.Select(e => e(list)).OfType<ListUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListUpdatedDomainEvent>(e =>
                e.UpdatedById == 2 && e.UpdatedByUserName == "bob" && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndWidthAreTheSame()
    {
        var list = List.Create(1, 2, "Title", 300, "rank", CreateUser(), DateTimeOffset.UtcNow);
        list.ClearDomainEvents();

        var changed = list.Update("Title", 300, CreateUser(2, "bob"), DateTimeOffset.UtcNow, "new-conn");

        changed.Should().BeFalse();
        list.UpdatedById.Should().BeNull();
        list.UpdatedAt.Should().BeNull();
        list.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Move_Should_UpdatePosition_And_RaiseEventWithMover()
    {
        var list = List.Create(1, 2, "Title", 300, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var movedBy = CreateUser(5, "bob");
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
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var list = List.Create(1, 2, "Title", 300, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var deletedBy = CreateUser(3, "carol");
        var now = DateTimeOffset.UtcNow;

        list.SoftDelete(deletedBy, now);

        list.IsDeleted.Should().BeTrue();
        list.DeletedById.Should().Be(3);
        list.DeletedAt.Should().Be(now);
        list.DeletedByCascade.Should().BeFalse();

        list.DomainEvents.Select(e => e(list)).OfType<ListDeletedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<ListDeletedDomainEvent>(e => e.DeletedById == 3 && e.DeletedByUserName == "carol");
    }
}
