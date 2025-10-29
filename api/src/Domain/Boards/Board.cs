using Snapflow.Common;
using Snapflow.Domain.BoardMembers;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Boards;

public class Board : Entity<int>
{
    public required string Title { get; set; }
    public string Description { get; set; } = "";

    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public virtual IUser CreatedBy { get; set; } = null!;

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public virtual IUser? UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public virtual IUser? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public virtual HashSet<BoardMember> Members { get; set; } = [];
    public virtual HashSet<Swimlane> Swimlanes { get; set; } = [];
    public virtual HashSet<List> Lists { get; set; } = [];
    public virtual HashSet<Card> Cards { get; set; } = [];
    public virtual HashSet<Tag> Tags { get; set; } = [];
}
