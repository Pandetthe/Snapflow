using FluentAssertions;
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

        board.Tags.Should().ContainSingle().Which.Should().BeSameAs(tag);
        tag.Title.Should().Be("Test Tag");
        tag.Color.Should().Be(TagColors.Red);
        tag.CreatedById.Should().Be(7);
        tag.CreatedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<TagCreatedDomainEvent>(e =>
                e.Title == "Test Tag" && e.Color == TagColors.Red &&
                e.CreatedById == 7 && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void CreateTag_Should_Fail_When_TitleIsTaken()
    {
        var board = CreateBoard();
        board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow);

        var result = board.CreateTag("Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        result.Error.Code.Should().Be("Tags.TitleNotUnique");
        board.Tags.Should().ContainSingle();
    }

    [Fact]
    public void CreateTag_Should_ReuseTitle_Of_DeletedTag()
    {
        var board = CreateBoard();
        var old = board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow).Value;
        board.DeleteTag(old.Id, 1, DateTimeOffset.UtcNow);

        var result = board.CreateTag("Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateTag_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Old", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value;
        var now = DateTimeOffset.UtcNow;

        var changed = board.UpdateTag(tag.Id, "New Title", TagColors.Green, 2, now, "new-conn");

        changed.Value.Should().BeTrue();
        tag.Title.Should().Be("New Title");
        tag.Color.Should().Be(TagColors.Green);
        tag.UpdatedById.Should().Be(2);
        tag.UpdatedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<TagUpdatedDomainEvent>(e =>
                e.Title == "New Title" && e.Color == TagColors.Green &&
                e.UpdatedById == 2 && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void UpdateTag_Should_ChangeNothing_When_TitleAndColourAreTheSame()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Title", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value;
        tag.ClearDomainEvents();

        var changed = board.UpdateTag(tag.Id, "Title", TagColors.Blue, 2, DateTimeOffset.UtcNow, "new-conn");

        changed.Value.Should().BeFalse();
        tag.UpdatedById.Should().BeNull();
        tag.UpdatedAt.Should().BeNull();
        tag.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateTag_Should_Fail_When_AnotherTagHasTheTitle()
    {
        var board = CreateBoard();
        Loaded.WithId(board.CreateTag("Bug", TagColors.Red, 1, DateTimeOffset.UtcNow).Value, 1);
        var tag = Loaded.WithId(board.CreateTag("Feature", TagColors.Blue, 1, DateTimeOffset.UtcNow).Value, 2);

        var result = board.UpdateTag(tag.Id, "Bug", TagColors.Blue, 1, DateTimeOffset.UtcNow);

        result.Error.Code.Should().Be("Tags.TitleNotUnique");
        tag.Title.Should().Be("Feature");
    }

    [Fact]
    public void DeleteTag_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var board = CreateBoard();
        var tag = board.CreateTag("Title", TagColors.Red, 1, DateTimeOffset.UtcNow).Value;
        var now = DateTimeOffset.UtcNow;

        var result = board.DeleteTag(tag.Id, 3, now);

        result.IsSuccess.Should().BeTrue();
        tag.IsDeleted.Should().BeTrue();
        tag.DeletedById.Should().Be(3);
        tag.DeletedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagDeletedDomainEvent>().Should().ContainSingle()
            .Which.DeletedById.Should().Be(3);
    }

    [Fact]
    public void DeleteTag_Should_Fail_When_TagIsMissing()
    {
        var board = CreateBoard();

        var result = board.DeleteTag(42, 1, DateTimeOffset.UtcNow);

        result.Error.Code.Should().Be("Tags.NotFound");
    }
}
