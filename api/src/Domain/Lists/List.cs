using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Lists;

public class List : Entity<int>
{
    public int BoardId { get; set; }
    public virtual Board Board { get; set; } = null!;

    public int SwimlaneId { get; set; }
    public virtual Swimlane Swimlane { get; set; } = null!;

    public required string Title { get; set; }

    public required string Rank { get; set; }
    public int? Width { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public virtual IUser CreatedBy { get; set; } = null!;

    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public IUser? UpdatedBy { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public IUser? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool DeletedByCascade { get; set; }

    public virtual ICollection<Card> Cards { get; set; } = [];
}
