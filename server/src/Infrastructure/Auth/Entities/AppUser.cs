using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapflow.Infrastructure.Auth.Entities;

#pragma warning disable CS8766
public sealed class AppUser : IdentityUser<int>, IUser
{
    private readonly List<Func<object, IDomainEvent>> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();

    public void Raise(Func<AppUser, IDomainEvent> blueprint) =>
        _domainEvents.Add(obj => blueprint((AppUser)obj));

    public void ClearDomainEvents() => _domainEvents.Clear();
}
