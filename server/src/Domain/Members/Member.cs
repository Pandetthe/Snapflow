using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Domain.Members;

public class Member : Entity<Member>
{
    public Member() { }
    
    public int BoardId { get; private set; }
    public virtual Board Board { get; private set; } = null!;

    public int UserId { get; private set; }
    public virtual IUser User { get; private set; } = null!;

    public MemberRole Role { get; private set; }

    public static Member Create(int boardId, int userId, MemberRole role, string? connectionId = null)
    {
        var member = new Member
        {
            BoardId = boardId,
            UserId = userId,
            Role = role
        };

        member.Raise(m => new MemberCreatedDomainEvent(m.UserId, m.BoardId, m.Role, connectionId));

        return member;
    }

    public void UpdateRole(MemberRole newRole, string? connectionId = null)
    {
        if (Role == newRole) return;

        var oldRole = Role;
        Role = newRole;

        Raise(m => new MemberRoleChangedDomainEvent(m.UserId, m.BoardId, oldRole, Role, connectionId));
    }

    public void Remove(string? connectionId = null)
    {
        Raise(m => new MemberRemovedDomainEvent(m.UserId, m.BoardId, connectionId));
    }
}
