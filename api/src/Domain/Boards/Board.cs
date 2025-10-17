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
    public virtual User? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public virtual User? UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public virtual User? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public HashSet<BoardMember>? Members { get; init; }
    public HashSet<Swimlane>? Swimlanes { get; init; }
    public HashSet<List>? Lists { get; init; }
    public HashSet<Card>? Cards { get; init; }
    public HashSet<Tag>? Tags { get; init; }
}
