using Snapflow.Common;

namespace Snapflow.Domain.Members;

public sealed record MemberRemovedDomainEvent(int UserId, int BoardId) : IDomainEvent;
