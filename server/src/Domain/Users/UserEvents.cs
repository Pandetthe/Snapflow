using Snapflow.Common;

namespace Snapflow.Domain.Users;

public sealed record UserSignedUpDomainEvent(int UserId, string? ConnectionId = null) : IDomainEvent;