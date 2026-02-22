using FluentAssertions;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.UnitTests.Domain;

public sealed class SwimlaneTests
{
    [Fact]
    public void Create_Should_InitializeSwimlane_And_RaiseEvent()
    {
        var boardId = 1;
        var title = "Test Swimlane";
        int? height = 100;
        var rank = "000000000001";
        var createdById = 1;
        var now = DateTimeOffset.UtcNow;

        var swimlane = Swimlane.Create(boardId, title, height, rank, createdById, now, "conn-id");

        swimlane.BoardId.Should().Be(boardId);
        swimlane.Title.Should().Be(title);
        swimlane.Height.Should().Be(height);
        swimlane.Rank.Should().Be(rank);
        swimlane.CreatedById.Should().Be(createdById);
        swimlane.CreatedAt.Should().Be(now);

        swimlane.DomainEvents.Select(e => e(swimlane)).Should().ContainSingle(e => e is SwimlaneCreatedDomainEvent);
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var swimlane = Swimlane.Create(1, "Old", 100, "rank", 1, DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        int? newHeight = 200;
        var updaterId = 2;
        var now = DateTimeOffset.UtcNow;

        swimlane.Update(newTitle, newHeight, updaterId, now, "new-conn");

        swimlane.Title.Should().Be(newTitle);
        swimlane.Height.Should().Be(newHeight);
        swimlane.UpdatedById.Should().Be(updaterId);
        swimlane.UpdatedAt.Should().Be(now);
        
        swimlane.DomainEvents.Select(e => e(swimlane)).Should().Contain(e => e is SwimlaneUpdatedDomainEvent);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var swimlane = Swimlane.Create(1, "Title", 100, "rank", 1, DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        swimlane.SoftDelete(deleterId, now);

        swimlane.IsDeleted.Should().BeTrue();
        swimlane.DeletedById.Should().Be(deleterId);
        swimlane.DeletedAt.Should().Be(now);
        swimlane.DeletedByCascade.Should().BeFalse();

        swimlane.DomainEvents.Select(e => e(swimlane)).Should().Contain(e => e is SwimlaneDeletedDomainEvent);
    }
}
