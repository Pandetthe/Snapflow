using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<int>, IUser
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
