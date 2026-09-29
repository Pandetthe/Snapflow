using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;

namespace Snapflow.UnitTests.Domain;

public sealed class TagTests
{
    private static Board CreateBoard() =>
        Board.Create("Board", "", BoardVisibility.Private, 1, DateTimeOffset.UtcNow);

    [Fact]
    public void CreateTag_Should_InitializeTag_And_RaiseEventWithCreator()
    {
        var board = CreateBoard();
        var now = DateTimeOffset.UtcNow;

        var tag = board.CreateTag("Test Tag", TagColors.Red, 7, now, "conn-id").Value;

        Assert.Same(tag, Assert.Single(board.Tags));
        Assert.Equal("Test Tag", tag.Title);
        Assert.Equal(TagColors.Red, tag.Color);
        Assert.Equal(7, tag.CreatedById);
        Assert.Equal(now, tag.CreatedAt);

        TagCreatedDomainEvent raised = Assert.Single(tag.DomainEvents.Select(e => e(tag)).OfType<TagCreatedDomainEvent>());
        Assert.True(raised.Title == "Test Tag" && raised.Color == TagColors.Red && raised.CreatedById == 7 && raised.ConnectionId == "conn-id");
    }

    [Fact]
    public void CreateTag_Should_Fail_When_TitleIsTaken()
    {
        var board = CreateBoard();
        board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow);

        var result = board.CreateTag("Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        Assert.Equal("Tags.TitleNotUnique", result.Error.Code);
        Assert.Single(board.Tags);
    }

    [Fact]
    public void CreateTag_Should_ReuseTitle_Of_DeletedTag()
    {
        var board = CreateBoard();
        var old = board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow).Value;
        board.DeleteTag(old.Id, 1, DateTimeOffset.UtcNow);

        var result = board.CreateTag("Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateTag_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Old", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value;
        var now = DateTimeOffset.UtcNow;

        var changed = board.UpdateTag(tag.Id, "New Title", TagColors.Green, 2, now, "new-conn");

        Assert.True(changed.Value);
        Assert.Equal("New Title", tag.Title);
        Assert.Equal(TagColors.Green, tag.Color);
        Assert.Equal(2, tag.UpdatedById);
        Assert.Equal(now, tag.UpdatedAt);

        TagUpdatedDomainEvent raised = Assert.Single(tag.DomainEvents.Select(e => e(tag)).OfType<TagUpdatedDomainEvent>());
        Assert.True(raised.Title == "New Title" && raised.Color == TagColors.Green && raised.UpdatedById == 2 && raised.ConnectionId == "new-conn");
    }

    [Fact]
    public void UpdateTag_Should_ChangeNothing_When_TitleAndColourAreTheSame()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Title", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value;
        tag.ClearDomainEvents();

        var changed = board.UpdateTag(tag.Id, "Title", TagColors.Blue, 2, DateTimeOffset.UtcNow, "new-conn");

        Assert.False(changed.Value);
        Assert.Null(tag.UpdatedById);
        Assert.Null(tag.UpdatedAt);
        Assert.Empty(tag.DomainEvents);
    }

    [Fact]
    public void UpdateTag_Should_Fail_When_AnotherTagHasTheTitle()
    {
        var board = CreateBoard();
        Loaded.WithId(board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow).Value, 1);
        var tag = Loaded.WithId(board.CreateTag("Feature", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value, 2);

        var result = board.UpdateTag(tag.Id, "Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        Assert.Equal("Tags.TitleNotUnique", result.Error.Code);
        Assert.Equal("Feature", tag.Title);
    }

    [Fact]
    public void DeleteTag_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Title", TagColors.Red, 1, DateTimeOffset.UtcNow).Value;
        var now = DateTimeOffset.UtcNow;

        var result = board.DeleteTag(tag.Id, 3, now);

        Assert.True(result.IsSuccess);
        Assert.True(tag.IsDeleted);
        Assert.Equal(3, tag.DeletedById);
        Assert.Equal(now, tag.DeletedAt);

        Assert.Equal(3, Assert.Single(tag.DomainEvents.Select(e => e(tag)).OfType<TagDeletedDomainEvent>()).DeletedById);
    }

    [Fact]
    public void DeleteTag_Should_Fail_When_TagIsMissing()
    {
        var board = CreateBoard();

        var result = board.DeleteTag(42, 1, DateTimeOffset.UtcNow);

        Assert.Equal("Tags.NotFound", result.Error.Code);
    }
}
