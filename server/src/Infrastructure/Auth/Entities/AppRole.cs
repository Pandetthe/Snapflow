using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Roles;

namespace Snapflow.Infrastructure.Auth.Entities;

public sealed class AppRole : IdentityRole<int>, IRole
{
    private readonly List<Func<object, IDomainEvent>> _domainEvents = [];

    public IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();

    public void Raise(Func<AppUser, IDomainEvent> blueprint) =>
        _domainEvents.Add(obj => blueprint((AppUser)obj));

    public void ClearDomainEvents() => _domainEvents.Clear();
}
