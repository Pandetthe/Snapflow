using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Tags;

public class Tag : Entity<int, Tag>, ISoftDeletable
{
    private readonly List<Card> _cards = [];

    private Tag() { }

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

    public virtual IReadOnlyCollection<Card> Cards => _cards;

    internal static Tag Create(Board board, string title, TagColors color, int createdById, DateTimeOffset createdAt, string? connectionId)
    {
        var tag = new Tag
        {
            Board = board,
            BoardId = board.Id,
            Title = title,
            Color = color,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        tag.Raise(t => new TagCreatedDomainEvent(t.Id, t.BoardId, t.Title, t.Color,
            createdById, connectionId));

        return tag;
    }

    internal bool Update(string title, TagColors color, int updatedById, DateTimeOffset updatedAt, string? connectionId)
    {
        if (Title == title && Color == color)
            return false;

        Title = title;
        Color = color;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(t => new TagUpdatedDomainEvent(t.Id, t.BoardId, t.Title, t.Color,
            updatedById, connectionId));
        return true;
    }

    internal void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;

        Raise(t => new TagDeletedDomainEvent(t.Id, t.BoardId,
            deletedById, connectionId));
    }
}
