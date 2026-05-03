using Snapflow.Common;

namespace Snapflow.Domain.Users;

public sealed record UserSignedUpDomainEvent(int UserId, string? ConnectionId = null) : IDomainEvent;

public sealed record UserProfileUpdatedDomainEvent(int UserId, string? ConnectionId = null) : IDomainEvent;

public sealed record UserDeletedDomainEvent(int UserId, string? ConnectionId = null) : IDomainEvent;