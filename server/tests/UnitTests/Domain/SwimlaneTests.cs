using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class SwimlaneTests
{
    private static IUser CreateUser(int id = 1, string userName = "john")
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.UserName.Returns(userName);
        return user;
    }

    [Fact]
    public void Create_Should_InitializeSwimlane_And_RaiseEventWithCreator()
    {
        var boardId = 1;
        var title = "Test Swimlane";
        int? height = 100;
        var rank = "000000000001";
        var createdBy = CreateUser(7, "alice");
        var now = DateTimeOffset.UtcNow;

        var swimlane = Swimlane.Create(boardId, title, height, rank, createdBy, now, "conn-id");

        swimlane.BoardId.Should().Be(boardId);
        swimlane.Title.Should().Be(title);
        swimlane.Height.Should().Be(height);
        swimlane.Rank.Should().Be(rank);
        swimlane.CreatedById.Should().Be(7);
        swimlane.CreatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneCreatedDomainEvent>(e =>
                e.CreatedById == 7 && e.CreatedByUserName == "alice" && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var swimlane = Swimlane.Create(1, "Old", 100, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        int? newHeight = 200;
        var updatedBy = CreateUser(2, "bob");
        var now = DateTimeOffset.UtcNow;

        swimlane.Update(newTitle, newHeight, updatedBy, now, "new-conn");

        swimlane.Title.Should().Be(newTitle);
        swimlane.Height.Should().Be(newHeight);
        swimlane.UpdatedById.Should().Be(2);
        swimlane.UpdatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneUpdatedDomainEvent>(e =>
                e.UpdatedById == 2 && e.UpdatedByUserName == "bob" && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Move_Should_UpdateRank_And_RaiseEventWithMover()
    {
        var swimlane = Swimlane.Create(1, "Title", 100, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var movedBy = CreateUser(5, "bob");
        var now = DateTimeOffset.UtcNow;

        swimlane.Move("rank2", movedBy, now, "move-conn");

        swimlane.Rank.Should().Be("rank2");
        swimlane.UpdatedById.Should().Be(5);
        swimlane.UpdatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneMovedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneMovedDomainEvent>(e =>
                e.Rank == "rank2" && e.MovedById == 5 && e.MovedByUserName == "bob" && e.ConnectionId == "move-conn");
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var swimlane = Swimlane.Create(1, "Title", 100, "rank", CreateUser(), DateTimeOffset.UtcNow);
        var deletedBy = CreateUser(3, "carol");
        var now = DateTimeOffset.UtcNow;

        swimlane.SoftDelete(deletedBy, now);

        swimlane.IsDeleted.Should().BeTrue();
        swimlane.DeletedById.Should().Be(3);
        swimlane.DeletedAt.Should().Be(now);
        swimlane.DeletedByCascade.Should().BeFalse();

        swimlane.DomainEvents.Select(e => e(swimlane)).OfType<SwimlaneDeletedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<SwimlaneDeletedDomainEvent>(e => e.DeletedById == 3 && e.DeletedByUserName == "carol");
    }
}
