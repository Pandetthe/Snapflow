using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Members;

public class Member : Entity<Member>
{
    private Member() { }

    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;

    public int UserId { get; private set; }
    public virtual IUser User { get; private set; } = null!;

    public MemberRole Role { get; private set; }

    internal static Member Create(Board board, int userId, MemberRole role, string? connectionId)
    {
        var member = new Member
        {
            Board = board,
            BoardId = board.Id,
            UserId = userId,
            Role = role
        };

        member.Raise(m => new MemberCreatedDomainEvent(m.UserId, m.BoardId, m.Role, connectionId));

        return member;
    }

    internal void ChangeRole(MemberRole newRole, string? connectionId)
    {
        if (Role == newRole)
            return;

        var oldRole = Role;
        Role = newRole;

        Raise(m => new MemberRoleChangedDomainEvent(m.UserId, m.BoardId, oldRole, Role, connectionId));
    }

    internal void MarkRemoved(string? connectionId) =>
        Raise(m => new MemberRemovedDomainEvent(m.UserId, m.BoardId, connectionId));
}
