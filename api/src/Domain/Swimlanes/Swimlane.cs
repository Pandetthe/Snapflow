using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Swimlanes;

public class Swimlane : Entity<int>
{
    public required int BoardId { get; set; }
    public Board? Board { get; set; }

    public required string Title { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public required int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public User? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public HashSet<List>? Lists { get; set; }
    public HashSet<Card>? Cards { get; set; }
}
