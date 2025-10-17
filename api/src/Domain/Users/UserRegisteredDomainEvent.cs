using Snapflow.Common;

namespace Snapflow.Domain.Users;

public sealed record UserRegisteredDomainEvent : IDomainEvent<User>
{
    public int Id { get; private set; }

    public void SetEntity(User entity) => Id = entity.Id;
}
