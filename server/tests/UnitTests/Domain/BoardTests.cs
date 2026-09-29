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

        Assert.Equal(title, board.Title);
        Assert.Equal(description, board.Description);
        Assert.Equal(createdById, board.CreatedById);
        Assert.Equal(now, board.CreatedAt);
        
        var owner = Assert.Single(board.Members);
        Assert.Equal(createdById, owner.UserId);
        Assert.Equal(MemberRole.Owner, owner.Role);

        Assert.Single(board.DomainEvents.Select(e => e(board)), e => e is BoardCreatedDomainEvent);
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

        Assert.Equal(newTitle, board.Title);
        Assert.Equal(newDesc, board.Description);
        Assert.Equal(updaterId, board.UpdatedById);
        Assert.Equal(now, board.UpdatedAt);
        
        Assert.Contains(board.DomainEvents.Select(e => e(board)), e => e is BoardUpdatedDomainEvent);
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndDescriptionAreTheSame()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        board.ClearDomainEvents();

        var changed = board.Update("Title", "Desc", 2, DateTimeOffset.UtcNow, "new-conn");

        Assert.False(changed);
        Assert.Null(board.UpdatedById);
        Assert.Null(board.UpdatedAt);
        Assert.Empty(board.DomainEvents);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        board.SoftDelete(deleterId, now);

        Assert.True(board.IsDeleted);
        Assert.Equal(deleterId, board.DeletedById);
        Assert.Equal(now, board.DeletedAt);

        Assert.Contains(board.DomainEvents.Select(e => e(board)), e => e is BoardDeletedDomainEvent);
    }

    [Theory]
    [InlineData(BoardVisibility.Private)]
    [InlineData(BoardVisibility.Public)]
    public void Create_Should_SetVisibility(BoardVisibility visibility)
    {
        var board = Board.Create("Title", "Desc", visibility, 1, DateTimeOffset.UtcNow);

        Assert.Equal(visibility, board.Visibility);
    }

    [Fact]
    public void ChangeVisibility_Should_UpdateVisibility_And_RaiseEvent()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        board.ChangeVisibility(BoardVisibility.Public, 1, now, "conn-id");

        Assert.Equal(BoardVisibility.Public, board.Visibility);
        Assert.Equal(1, board.UpdatedById);
        Assert.Equal(now, board.UpdatedAt);
        BoardVisibilityChangedDomainEvent raised = Assert.Single(board.DomainEvents.Select(e => e(board)).OfType<BoardVisibilityChangedDomainEvent>());
        Assert.True(raised.OldVisibility == BoardVisibility.Private && raised.NewVisibility == BoardVisibility.Public);
    }

    [Fact]
    public void ChangeVisibility_Should_DoNothing_When_VisibilityIsTheSame()
    {
        var board = Board.Create("Title", "Desc", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);

        board.ChangeVisibility(BoardVisibility.Private, 2, DateTimeOffset.UtcNow);

        Assert.Null(board.UpdatedById);
        Assert.DoesNotContain(board.DomainEvents.Select(e => e(board)), e => e is BoardVisibilityChangedDomainEvent);
    }
}
