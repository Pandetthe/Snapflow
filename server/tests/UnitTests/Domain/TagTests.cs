using FluentAssertions;
using NSubstitute;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.UnitTests.Domain;

public sealed class TagTests
{
    private static IUser CreateUser(int id = 1, string userName = "john")
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.UserName.Returns(userName);
        return user;
    }

    [Fact]
    public void Create_Should_InitializeTag_And_RaiseEventWithCreator()
    {
        var boardId = 1;
        var title = "Test Tag";
        var color = TagColors.Red;
        var createdBy = CreateUser(7, "alice");
        var now = DateTimeOffset.UtcNow;

        var tag = Tag.Create(boardId, title, color, createdBy, now, "conn-id");

        tag.BoardId.Should().Be(boardId);
        tag.Title.Should().Be(title);
        tag.Color.Should().Be(color);
        tag.CreatedById.Should().Be(7);
        tag.CreatedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagCreatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<TagCreatedDomainEvent>(e =>
                e.Title == title && e.Color == color &&
                e.CreatedById == 7 && e.CreatedByUserName == "alice" && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEventWithUpdater()
    {
        var tag = Tag.Create(1, "Old", TagColors.Blue, CreateUser(), DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        var newColor = TagColors.Green;
        var updatedBy = CreateUser(2, "bob");
        var now = DateTimeOffset.UtcNow;

        tag.Update(newTitle, newColor, updatedBy, now, "new-conn");

        tag.Title.Should().Be(newTitle);
        tag.Color.Should().Be(newColor);
        tag.UpdatedById.Should().Be(2);
        tag.UpdatedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagUpdatedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<TagUpdatedDomainEvent>(e =>
                e.Title == newTitle && e.Color == newColor &&
                e.UpdatedById == 2 && e.UpdatedByUserName == "bob" && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void Update_Should_ChangeNothing_When_TitleAndColourAreTheSame()
    {
        var tag = Tag.Create(1, "Title", TagColors.Blue, CreateUser(), DateTimeOffset.UtcNow);
        tag.ClearDomainEvents();

        var changed = tag.Update("Title", TagColors.Blue, CreateUser(2, "bob"), DateTimeOffset.UtcNow, "new-conn");

        changed.Should().BeFalse();
        tag.UpdatedById.Should().BeNull();
        tag.UpdatedAt.Should().BeNull();
        tag.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEventWithDeleter()
    {
        var tag = Tag.Create(1, "Title", TagColors.Red, CreateUser(), DateTimeOffset.UtcNow);
        var deletedBy = CreateUser(3, "carol");
        var now = DateTimeOffset.UtcNow;

        tag.SoftDelete(deletedBy, now);

        tag.IsDeleted.Should().BeTrue();
        tag.DeletedById.Should().Be(3);
        tag.DeletedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).OfType<TagDeletedDomainEvent>().Should().ContainSingle()
            .Which.Should().Match<TagDeletedDomainEvent>(e =>
                e.DeletedById == 3 && e.DeletedByUserName == "carol");
    }
}
