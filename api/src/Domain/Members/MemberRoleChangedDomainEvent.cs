using Snapflow.Common;

namespace Snapflow.Domain.Members;

public sealed record MemberRoleChangedDomainEvent(
    int UserId, 
    int BoardId,
    MemberRole 
    OldRole, 
    MemberRole NewRole) : IDomainEvent;
