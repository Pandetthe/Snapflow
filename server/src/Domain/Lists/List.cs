using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Ranking;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Lists;

public class List : Entity<int, List>, IRankable
{
    public List() { }
    
    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;

    public int SwimlaneId { get; private set; }
    public virtual Swimlane Swimlane { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Rank { get; set; } = null!;
    public int? Width { get; private set; }

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

    public virtual ICollection<Card> Cards { get; private set; } = [];

    public static List Create(int boardId, int swimlaneId, string title, int? width, string rank, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var list = new List
        {
            BoardId = boardId,
            SwimlaneId = swimlaneId,
            Title = title,
            Width = width,
            Rank = rank,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        list.Raise(l => new ListCreatedDomainEvent(l.Id, l.BoardId, l.SwimlaneId, l.Title, l.Width, l.Rank, connectionId));

        return list;
    }

    public void Update(string title, int? width, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Title = title;
        Width = width;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(l => new ListUpdatedDomainEvent(l.Id, l.BoardId, l.Title, l.Width, connectionId));
    }

    public void Move(int swimlaneId, string rank, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        SwimlaneId = swimlaneId;
        Rank = rank;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(l => new ListMovedDomainEvent(Id, BoardId, SwimlaneId, Rank, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;
        DeletedByCascade = false;

        Raise(l => new ListDeletedDomainEvent(l.Id, l.BoardId, connectionId));
    }
}
