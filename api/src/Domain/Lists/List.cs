using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Lists;

public class List : Entity<int>
{
    public required int BoardId { get; set; }
    public virtual Board? Board { get; set; }

    public required int SwimlaneId { get; set; }
    public virtual Swimlane? Swimlane { get; set; }

    public required string Title { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public virtual IUser? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public IUser? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public IUser? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public HashSet<Card> Cards { get; init; } = [];
}
