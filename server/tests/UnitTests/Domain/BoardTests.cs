using FluentAssertions;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.UnitTests.Domain;

public sealed class BoardTests
{
    [Fact]
    public void Create_Should_InitializeBoard_And_RaiseEvent_And_AddOwner()
    {
        var title = "Test Board";
        var description = "Test Description";
        var createdById = 1;
        var now = DateTimeOffset.UtcNow;

        var board = Board.Create(title, description, BoardVisibility.Private, createdById, now, "conn-id");

        board.Title.Should().Be(title);
        board.Description.Should().Be(description);
        board.CreatedById.Should().Be(createdById);
        board.CreatedAt.Should().Be(now);
        
        board.Members.Should().HaveCount(1);
        board.Members.First().UserId.Should().Be(createdById);
        board.Members.First().Role.Should().Be(MemberRole.Owner);

        board.DomainEvents.Select(e => e(board)).Should().ContainSingle(e => e is BoardCreatedDomainEvent);
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var board = Board.Create("Old", "Old Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        var newDesc = "New Desc";
        var updaterId = 2;
        var now = DateTimeOffset.UtcNow;

        board.Update(newTitle, newDesc, updaterId, now, "new-conn");

        board.Title.Should().Be(newTitle);
        board.Description.Should().Be(newDesc);
        board.UpdatedById.Should().Be(updaterId);
        board.UpdatedAt.Should().Be(now);
        
        board.DomainEvents.Select(e => e(board)).Should().Contain(e => e is BoardUpdatedDomainEvent);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        board.SoftDelete(deleterId, now);

        board.IsDeleted.Should().BeTrue();
        board.DeletedById.Should().Be(deleterId);
        board.DeletedAt.Should().Be(now);

        board.DomainEvents.Select(e => e(board)).Should().Contain(e => e is BoardDeletedDomainEvent);
    }

    [Theory]
    [InlineData(BoardVisibility.Private)]
    [InlineData(BoardVisibility.Public)]
    public void Create_Should_SetVisibility(BoardVisibility visibility)
    {
        var board = Board.Create("Title", "Desc", visibility, 1, DateTimeOffset.UtcNow);

        board.Visibility.Should().Be(visibility);
    }

    [Fact]
    public void ChangeVisibility_Should_UpdateVisibility_And_RaiseEvent()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        board.ChangeVisibility(BoardVisibility.Public, 1, now, "conn-id");

        board.Visibility.Should().Be(BoardVisibility.Public);
        board.UpdatedById.Should().Be(1);
        board.UpdatedAt.Should().Be(now);
        board.DomainEvents.Select(e => e(board)).OfType<BoardVisibilityChangedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<BoardVisibilityChangedDomainEvent>(e =>
                e.OldVisibility == BoardVisibility.Private && e.NewVisibility == BoardVisibility.Public);
    }

    [Fact]
    public void ChangeVisibility_Should_DoNothing_When_VisibilityIsTheSame()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);

        board.ChangeVisibility(BoardVisibility.Private, 2, DateTimeOffset.UtcNow);

        board.UpdatedById.Should().BeNull();
        board.DomainEvents.Select(e => e(board)).Should().NotContain(e => e is BoardVisibilityChangedDomainEvent);
    }
}
