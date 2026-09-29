using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using Snapflow.Domain.Ranking;

namespace Snapflow.Domain.Cards;

public class Card : Entity<int, Card>, IRankable, ICascadeSoftDeletable
{
    private readonly List<Tag> _tags = [];

    private Card() { }
    
    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;
    public int SwimlaneId { get; private set; }
    public virtual Swimlane Swimlane { get; private set; } = null!;
    public int ListId { get; private set; }
    public virtual List List { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = "";
    public string Rank { get; private set; } = null!;

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
    public bool DeletedByCascade { get; private set; }

    public virtual IReadOnlyCollection<Tag> Tags => _tags;

    public static Card Create(int boardId, int swimlaneId, int listId, string title, string description, string rank, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var card = new Card
        {
            BoardId = boardId,
            SwimlaneId = swimlaneId,
            ListId = listId,
            Title = title,
            Description = description,
            Rank = rank,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        card.Raise(c => new CardCreatedDomainEvent(c.Id, c.BoardId, c.SwimlaneId, c.ListId, c.Title, c.Description, c.Rank,
            c.CreatedAt, c.CreatedById, connectionId));

        return card;
    }

    /// <summary>Changes the card. Returns false, leaving it untouched, when it already reads that way.</summary>
    public bool Update(string title, string description, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        if (Title == title && Description == description)
            return false;

        Title = title;
        Description = description;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(c => new CardUpdatedDomainEvent(Id, BoardId, Title, Description, updatedById, connectionId));
        return true;
    }

    public void Move(int listId, int swimlaneId, string rank, int movedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        ListId = listId;
        SwimlaneId = swimlaneId;
        Rank = rank;
        UpdatedById = movedById;
        UpdatedAt = updatedAt;

        Raise(c => new CardMovedDomainEvent(Id, BoardId, ListId, Rank, movedById, connectionId));
    }

    public Result AddTag(Tag tag, int addedById, string? connectionId = null)
    {
        if (tag.BoardId != BoardId)
            return TagErrors.NotFound(tag.Id);
        if (_tags.Any(t => t.Id == tag.Id))
            return TagErrors.AlreadyOnCard(tag.Id, Id);

        _tags.Add(tag);
        Raise(c => new CardTagAddedDomainEvent(c.Id, tag.Id, c.BoardId, addedById, connectionId));
        return Result.Success();
    }

    public Result RemoveTag(Tag tag, int removedById, string? connectionId = null)
    {
        Tag? existing = _tags.FirstOrDefault(t => t.Id == tag.Id);
        if (existing == null)
            return TagErrors.NotOnCard(tag.Id, Id);

        _tags.Remove(existing);
        Raise(c => new CardTagRemovedDomainEvent(c.Id, tag.Id, c.BoardId, removedById, connectionId));
        return Result.Success();
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;
        DeletedByCascade = false;

        Raise(c => new CardDeletedDomainEvent(Id, BoardId, deletedById, connectionId));
    }
}