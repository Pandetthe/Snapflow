using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Ranking;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Swimlanes;

public class Swimlane : Entity<int, Swimlane>, IRankable
{
    public Swimlane() { }
    
    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Rank { get; set; } = null!;
    public int? Height { get; private set; }

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

    public virtual ICollection<List> Lists { get; private set; } = [];
    public virtual ICollection<Card> Cards { get; private set; } = [];

    public static Swimlane Create(int boardId, string title, int? height, string rank, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var swimlane = new Swimlane
        {
            BoardId = boardId,
            Title = title,
            Height = height,
            Rank = rank,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        swimlane.Raise(s => new SwimlaneCreatedDomainEvent(s.Id, s.BoardId, s.Title, s.Height, s.Rank, connectionId));

        return swimlane;
    }

    public void Update(string title, int? height, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Title = title;
        Height = height;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(s => new SwimlaneUpdatedDomainEvent(s.Id, s.BoardId, s.Title, s.Height, connectionId));
    }

    public void Move(string rank, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Rank = rank;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(s => new SwimlaneMovedDomainEvent(Id, BoardId, Rank, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;
        DeletedByCascade = false;

        Raise(s => new SwimlaneDeletedDomainEvent(s.Id, s.BoardId, connectionId));
    }
}
