using FluentAssertions;
using Snapflow.Domain.Tags;

namespace Snapflow.UnitTests.Domain;

public sealed class TagTests
{
    [Fact]
    public void Create_Should_InitializeTag_And_RaiseEvent()
    {
        var boardId = 1;
        var title = "Test Tag";
        var color = TagColors.Red;
        var createdById = 1;
        var now = DateTimeOffset.UtcNow;

        var tag = Tag.Create(boardId, title, color, createdById, now, "conn-id");

        tag.BoardId.Should().Be(boardId);
        tag.Title.Should().Be(title);
        tag.Color.Should().Be(color);
        tag.CreatedById.Should().Be(createdById);
        tag.CreatedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).Should().ContainSingle(e => e is TagCreatedDomainEvent);
    }

    [Fact]
    public void Update_Should_UpdateProperties_And_RaiseEvent()
    {
        var tag = Tag.Create(1, "Old", TagColors.Blue, 1, DateTimeOffset.UtcNow);
        var newTitle = "New Title";
        var newColor = TagColors.Green;
        var updaterId = 2;
        var now = DateTimeOffset.UtcNow;

        tag.Update(newTitle, newColor, updaterId, now, "new-conn");

        tag.Title.Should().Be(newTitle);
        tag.Color.Should().Be(newColor);
        tag.UpdatedById.Should().Be(updaterId);
        tag.UpdatedAt.Should().Be(now);
        
        tag.DomainEvents.Select(e => e(tag)).Should().Contain(e => e is TagUpdatedDomainEvent);
    }

    [Fact]
    public void SoftDelete_Should_SetIsDeletedTrue_And_RaiseEvent()
    {
        var tag = Tag.Create(1, "Title", TagColors.Red, 1, DateTimeOffset.UtcNow);
        var deleterId = 1;
        var now = DateTimeOffset.UtcNow;

        tag.SoftDelete(deleterId, now);

        tag.IsDeleted.Should().BeTrue();
        tag.DeletedById.Should().Be(deleterId);
        tag.DeletedAt.Should().Be(now);

        tag.DomainEvents.Select(e => e(tag)).Should().Contain(e => e is TagDeletedDomainEvent);
    }
}
