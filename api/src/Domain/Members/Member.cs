using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Members;

public class Member : Entity
{
    public int BoardId { get; set; }
    public virtual Board Board { get; set; } = null!;

    public int UserId { get; set; }
    public virtual IUser User { get; set; } = null!;

    public required MemberRole Role { get; set; }
}
