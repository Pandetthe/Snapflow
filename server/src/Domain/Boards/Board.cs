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

    public virtual ICollection<Member> Members { get; private set; } = [];
    public virtual ICollection<Swimlane> Swimlanes { get; private set; } = [];
    public virtual ICollection<List> Lists { get; private set; } = [];
    public virtual ICollection<Card> Cards { get; private set; } = [];
    public virtual ICollection<Tag> Tags { get; private set; } = [];
}
