using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Cards;

public class Card : Entity<int>
{
    public required int BoardId { get; set; }
    public virtual Board? Board { get; set; }

    public required int SwimlaneId { get; set; }
    public virtual Swimlane? Swimlane { get; set; }

    public required int ListId { get; set; }
    public virtual List? List { get; set; }

    public required string Title { get; set; }
    public string Description { get; set; } = "";

    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public virtual IUser? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public virtual IUser? UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public virtual IUser? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public virtual HashSet<Tag>? Tags { get; set; }
}
