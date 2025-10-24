using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.BoardMembers;

public class BoardMember : Entity<int>
{
    public required int BoardId { get; set; }
    public virtual Board? Board { get; set; }

    public required int UserId { get; set; }
    public virtual IUser? User { get; set; }

    public required BoardRole Role { get; set; }
}
