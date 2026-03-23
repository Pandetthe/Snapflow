using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapflow.Infrastructure.Auth.Entities;

#pragma warning disable CS8766
public sealed class AppUser : IdentityUser<int>, IUser
{
    private readonly List<Func<object, IDomainEvent>> _domainEvents = [];

    public IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();

    public byte[]? AvatarData { get; set; }

    public string? AvatarContentType { get; set; }

    public string? AvatarUrl { get; set; }

    public AvatarType AvatarType { get; set; } = AvatarType.Generated;
    
    public static AppUser Create(string email, string userName)
    {
        var user = new AppUser
        {
            Email = email,
            UserName = userName
        };

        user.Raise(u => new UserSignedUpDomainEvent(u.Id));

        return user;
    }

    public void Raise(Func<AppUser, IDomainEvent> blueprint) =>
        _domainEvents.Add(obj => blueprint((AppUser)obj));

    public void ClearDomainEvents() => _domainEvents.Clear();
}
