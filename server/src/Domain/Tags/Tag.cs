using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Tags;

public class Tag : Entity<int, Tag>
{
    public Tag() { }

    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public TagColors Color { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedById { get; private set; }
    public virtual IUser CreatedBy { get; private set; } = null!;

    public DateTimeOffset? UpdatedAt { get; private set; }
    public int? UpdatedById { get; private set; }
    public virtual IUser? UpdatedBy { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }
    public int? DeletedById { get; private set; }
    public virtual IUser? DeletedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public virtual HashSet<Card> Cards { get; private set; } = [];

    public static Tag Create(int boardId, string title, TagColors color, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var tag = new Tag
        {
            BoardId = boardId,
            Title = title,
            Color = color,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        tag.Raise(t => new TagCreatedDomainEvent(t.Id, t.BoardId, t.Title, connectionId));

        return tag;
    }

    public void Update(string title, TagColors color, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Title = title;
        Color = color;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(t => new TagUpdatedDomainEvent(t.Id, t.BoardId, t.Title, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;

        Raise(t => new TagDeletedDomainEvent(t.Id, t.BoardId, connectionId));
    }
}
