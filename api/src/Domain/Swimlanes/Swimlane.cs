using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Swimlanes;

public class Swimlane : Entity<int>
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public required string Title { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public IUser CreatedBy { get; set; } = null!;

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public IUser? UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public IUser? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public virtual HashSet<List> Lists { get; set; } = [];
    public virtual HashSet<Card> Cards { get; set; } = [];
}
