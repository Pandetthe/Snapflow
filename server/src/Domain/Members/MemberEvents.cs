using Snapflow.Common;

namespace Snapflow.Domain.Members;

public sealed record MemberCreatedDomainEvent(
    int UserId,
    int BoardId,
    MemberRole Role,
    string? ConnectionId) : IDomainEvent;

public sealed record MemberRoleChangedDomainEvent(
    int UserId,
    int BoardId,
    MemberRole OldRole,
    MemberRole NewRole,
    string? ConnectionId) : IDomainEvent;

public sealed record MemberRemovedDomainEvent(
    int UserId,
    int BoardId,
    string? ConnectionId) : IDomainEvent;
