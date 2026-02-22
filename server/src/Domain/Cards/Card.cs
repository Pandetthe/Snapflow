using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using Snapflow.Domain.Ranking;

namespace Snapflow.Domain.Cards;

public class Card : Entity<int, Card>, IRankable
{
    public Card() { }
    
    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;
    public int SwimlaneId { get; private set; }
    public virtual Swimlane Swimlane { get; private set; } = null!;
    public int ListId { get; private set; }
    public virtual List List { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = "";
    public string Rank { get; set; } = null!;

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

    public virtual ICollection<Tag> Tags { get; private set; } = [];

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

        card.Raise(c => new CardCreatedDomainEvent(c.Id, c.BoardId, c.SwimlaneId, c.ListId, c.Title, c.Description, c.Rank, connectionId));

        return card;
    }

    public void Update(string title, string description, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Title = title;
        Description = description;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(c => new CardUpdatedDomainEvent(Id, BoardId, Title, Description, connectionId));
    }

    public void Move(int listId, int swimlaneId, string rank, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        ListId = listId;
        SwimlaneId = swimlaneId;
        Rank = rank;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(c => new CardMovedDomainEvent(Id, BoardId, ListId, Rank, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;
        DeletedByCascade = false;

        Raise(c => new CardDeletedDomainEvent(Id, BoardId, connectionId));
    }
}