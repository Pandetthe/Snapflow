using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.DomainEvents;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Persistence;

public sealed class AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDomainEventsDispatcher domainEventsDispatcher) 
    : IdentityDbContext<AppUser, AppRole, int>(options), IAppDbContext
{
    IQueryable<IUser> IAppDbContext.Users => Set<AppUser>().AsQueryable().Cast<IUser>();
    public DbSet<Board> Boards { get; private set; }
    public DbSet<Swimlane> Swimlanes { get; private set; }
    public DbSet<List> Lists { get; private set; }
    public DbSet<BoardMember> BoardMembers { get; private set; }
    public DbSet<Tag> Tags { get; private set; }
    public DbSet<Card> Cards { get; private set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                foreach (var domainEvent in entity.DomainEvents.OfType<IDomainEvent<IEntity>>())
                    domainEvent.SetEntity(entity);

                var events = entity.DomainEvents;
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();
        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
