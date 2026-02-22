using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Members;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Boards;

public class Board : Entity<int, Board>
{
    public Board() { }
    
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = "";

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

    public virtual ICollection<Member> Members { get; private set; } = [];
    public virtual ICollection<Swimlane> Swimlanes { get; private set; } = [];
    public virtual ICollection<List> Lists { get; private set; } = [];
    public virtual ICollection<Card> Cards { get; private set; } = [];
    public virtual ICollection<Tag> Tags { get; private set; } = [];

    public static Board Create(string title, string description, int createdById, DateTimeOffset createdAt, string? connectionId = null)
    {
        var board = new Board
        {
            Title = title,
            Description = description,
            CreatedById = createdById,
            CreatedAt = createdAt
        };

        board.Members.Add(Member.Create(board.Id, createdById, MemberRole.Owner, connectionId));

        board.Raise(b => new BoardCreatedDomainEvent(b.Id, b.Title, b.CreatedById, connectionId));

        return board;
    }

    public void Update(string title, string description, int updatedById, DateTimeOffset updatedAt, string? connectionId = null)
    {
        Title = title;
        Description = description;
        UpdatedById = updatedById;
        UpdatedAt = updatedAt;

        Raise(b => new BoardUpdatedDomainEvent(b.Id, b.Title, b.Description, connectionId));
    }

    public void SoftDelete(int deletedById, DateTimeOffset deletedAt, string? connectionId = null)
    {
        IsDeleted = true;
        DeletedById = deletedById;
        DeletedAt = deletedAt;

        Raise(b => new BoardDeletedDomainEvent(b.Id, connectionId));
    }
}
